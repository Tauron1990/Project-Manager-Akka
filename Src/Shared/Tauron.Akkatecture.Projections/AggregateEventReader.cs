using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Akka;
using Akka.Actor;
using Akka.Persistence.Query;
using Akka.Streams;
using Akka.Streams.Dsl;
using Akka.Util;
using Akkatecture.Aggregates;
using Akkatecture.Events;
using Akkatecture.Query;
using JetBrains.Annotations;
using LiquidProjections;
using LiquidProjections.Abstractions;
using EventEnvelope = Akka.Persistence.Query.EventEnvelope;

namespace Tauron.Akkatecture.Projections
{
    [PublicAPI]
    public sealed class EventReaderException : EventArgs
    {
        public EventReaderException(string tag, Exception exception)
        {
            Tag = tag;
            Exception = exception;
        }

        public string Tag { get; }

        public Exception Exception { get; }
    }

    [PublicAPI]
    public abstract class AggregateEventReader
    {
        public event EventHandler<EventReaderException>? OnReadError;

        #pragma warning disable EPS05
        public abstract IDisposable CreateSubscription(
            long? lastProcessedCheckpoint, Subscriber subscriber,
            #pragma warning restore EPS05
            string subscriptionId);

        protected virtual void ReadError(EventReaderException e)
        {
            OnReadError?.Invoke(this, e);
        }
    }

    [PublicAPI]
    public sealed class AggregateEventReader<TJournal> : AggregateEventReader
        where TJournal : IEventsByTagQuery, ICurrentEventsByTagQuery
    {
        private readonly ActorMaterializer _actorMaterializer;
        private readonly string _journalId;
        private readonly ActorSystem _system;

        public AggregateEventReader(ActorSystem system, string journalId)
        {
            _system = system;
            _journalId = journalId;
            _actorMaterializer = system.Materializer();
        }

        public override IDisposable CreateSubscription(
            long? lastProcessedCheckpoint, Subscriber subscriber,
            string subscriptionId)
        {
            string ExtractInfo() => subscriptionId[(subscriptionId.IndexOf('@') + 1)..];

            if (subscriptionId.StartsWith("Type"))
                return MakeAggregateSubscription(lastProcessedCheckpoint, subscriber, Type.GetType(ExtractInfo())!);
            if (subscriptionId.StartsWith("Tag"))
                return MakeTagAggregateSubscription(lastProcessedCheckpoint, subscriber, ExtractInfo());

            throw new ArgumentException($"Invalid Subscription Id Format: {subscriptionId}", nameof(subscriptionId));
        }

        private IDisposable MakeAggregateSubscription(
            in long? lastProcessedCheckpoint, Subscriber subscriber,
            Type aggregate)
        {
            var genericType = typeof(SubscriptionBuilder<>).MakeGenericType(aggregate);
            var subscriberInst =
                (SubscriptionBuilder)FastReflection.Shared.FastCreateInstance(
                    genericType,
                    _actorMaterializer,
                    _system,
                    _journalId,
                    subscriber)!;

            return subscriberInst.CreateSubscription(
                lastProcessedCheckpoint,
                (tag, exception) => ReadError(new EventReaderException(tag, exception)));
        }

        private IDisposable MakeTagAggregateSubscription(
            in long? lastProcessedCheckpoint, Subscriber subscriber,
            string tag)
        {
            return new TagBasedSubscriptionBuilder(_system, _journalId, _actorMaterializer, subscriber, tag)
               .CreateSubscription(
                    lastProcessedCheckpoint,
                    (s, exception) => ReadError(new EventReaderException(s, exception)));
        }

        private abstract class SubscriptionBuilder : IDisposable
        {
            private readonly string _exceptionInfo;
            private readonly AtomicBoolean _isCancel = new();

            private readonly ActorMaterializer _materializer;
            private readonly Subscriber _subscriber;

            private UniqueKillSwitch? _cancelable;
            private Action<string, Exception>? _errorHandler;
            private Task? _runner;

            protected SubscriptionBuilder(ActorMaterializer materializer, Subscriber subscriber, string exceptionInfo)
            {
                _materializer = materializer;
                _subscriber = subscriber;
                _exceptionInfo = exceptionInfo;
            }

            public void Dispose()
            {
                _isCancel.GetAndSet(newValue: true);
                _cancelable?.Shutdown();
                _runner?.Wait(TimeSpan.FromSeconds(20));
            }

            internal IDisposable CreateSubscription(in long? lastProcessedCheckpoint, Action<string, Exception> errorHandler)
            {
                _errorHandler = errorHandler;

                var source = CreateSource(Offset.Sequence(lastProcessedCheckpoint ?? 0))
                   .Select(ee => (ee.Event as IDomainEvent, ee.Offset))
                   .Where(de => de.Item1 != null)
                   .Batch(
                        20,
                        de => ImmutableList<(IDomainEvent, Offset)>.Empty.Add(de!),
                        (list, evt) => list.Add(evt!))
                   .Select(
                        de =>
                        {
                            var (domainEvent, offset) = de.Last();

                            return new Transaction
                                   {
                                       Checkpoint = ((Sequence)offset).Value,
                                       Id = EventId.New.Value,
                                       StreamId = domainEvent.GetIdentity().Value,
                                       TimeStampUtc = domainEvent.Timestamp.DateTime,
                                       Events = new List<LiquidProjections.EventEnvelope>(
                                           de
                                              .Select(
                                                   pair =>
                                                   {
                                                       var (evt, _) = pair;

                                                       return new LiquidProjections.EventEnvelope
                                                              {
                                                                  Body = evt,
                                                                  Headers = evt
                                                                     .Metadata
                                                                     .Select(p => Tuple.Create<string, object>(p.Key, p.Value))
                                                                     .ToDictionary(t => t.Item1, t => t.Item2)
                                                              };
                                                   }))
                                   };
                        })
                   .Batch(
                        5,
                        t => ImmutableList<Transaction>.Empty.Add(t),
                        (list, transaction) => list.Add(transaction))
                   .AlsoTo(
                        Sink.OnComplete<ImmutableList<Transaction>>(
                            () => _isCancel.GetAndSet(newValue: true),
                            e =>
                            {
                                _isCancel.GetAndSet(newValue: true);
                                errorHandler(_exceptionInfo, e);
                            }))
                   .ViaMaterialized(KillSwitches.Single<ImmutableList<Transaction>>(), (_, kill) => kill)
                   .PreMaterialize(_materializer);

                _cancelable = source.Item1;

                var sinkQueue = source.Item2.RunWith(
                    Sink.Queue<ImmutableList<Transaction>>()
                       .WithAttributes(new Attributes(new Attributes.InputBuffer(2, 2))),
                    _materializer);

                _runner = Run(sinkQueue);

                return this;
            }

            private async Task Run(ISinkQueue<ImmutableList<Transaction>> queue)
            {
                while (!_isCancel.Value)
                    try
                    {
                        var data = await queue.PullAsync();

                        if (data.HasValue)
                            await _subscriber.HandleTransactions(
                                data.Value,
                                new SubscriptionInfo
                                {
                                    Id = data.Value.Last().StreamId,
                                    Subscription = this
                                });
                        else
                            Thread.Sleep(1);
                    }
                    catch (Exception e)
                    {
                        _errorHandler?.Invoke(_exceptionInfo, e);
                    }
            }

            protected abstract Source<EventEnvelope, NotUsed> CreateSource(Offset offset);
        }

        private sealed class TagBasedSubscriptionBuilder : SubscriptionBuilder
        {
            // ReSharper disable once StaticMemberInGenericType
            private static readonly DomainEventReadAdapter Mapper = new();
            private readonly string _journalId;

            private readonly ActorSystem _system;
            private readonly string _tag;

            internal TagBasedSubscriptionBuilder(
                ActorSystem system, string journalId, ActorMaterializer materializer,
                Subscriber subscriber, string tag)
                : base(materializer, subscriber, tag)
            {
                _system = system;
                _journalId = journalId;
                _tag = tag;
            }

            protected override Source<EventEnvelope, NotUsed> CreateSource(Offset offset)
            {
                return PersistenceQuery.Get(_system).ReadJournalFor<TJournal>(_journalId)
                   .EventsByTag(_tag, offset)
                   .Select(
                        x =>
                        {
                            var domainEvent = Mapper.FromJournal(x.Event, string.Empty).Events.Single();

                            return new EventEnvelope(x.Offset, x.PersistenceId, x.SequenceNr, domainEvent, x.Timestamp);
                        });
            }
        }

        private sealed class SubscriptionBuilder<TAggregate> : SubscriptionBuilder
            where TAggregate : IAggregateRoot
        {
            private readonly string _journalId;
            private readonly ActorSystem _system;

            internal SubscriptionBuilder(
                ActorMaterializer materializer, ActorSystem system, string journalId,
                Subscriber subscriber)
                : base(materializer, subscriber, typeof(TAggregate).AssemblyQualifiedName ?? "Unkowne Type")
            {
                _system = system;
                _journalId = journalId;
            }

            protected override Source<EventEnvelope, NotUsed> CreateSource(Offset offset) => Consumer.Create(_system)
               .Using<TJournal>(_journalId)
               .EventsFromAggregate<TAggregate>(offset);
        }
    }
}