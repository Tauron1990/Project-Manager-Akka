using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Util;
using JetBrains.Annotations;
using Tauron.Operations;

namespace Tauron.Application.AkkaNode.Services.Reporting
{
    [PublicAPI]
    public sealed class Reporter
    {
        public static readonly Reporter Empty = new(ActorRefs.Nobody);

        public const string TimeoutError = nameof(TimeoutError);
        private readonly AtomicBoolean _compledCalled = new();

        private readonly IActorRef _reporter;

        public Reporter(IActorRef reporter) => _reporter = reporter;

        public bool IsCompled => _compledCalled.Value;

        public static Reporter CreateReporter(IActorRefFactory factory, string? name = null)
            => new(factory.ActorOf(Props.Create(() => new ReporterActor()).WithSupervisorStrategy(SupervisorStrategy.StoppingStrategy), name));

        public static IActorRef CreateListner(IActorRefFactory factory, Action<string> listner,
            Action<IOperationResult> onCompled, TimeSpan timeout, string? name = null)
            => factory.ActorOf(
                Props.Create(() => new Listner(listner, onCompled, timeout))
                    .WithSupervisorStrategy(SupervisorStrategy.StoppingStrategy), name);

        public static IActorRef CreateListner(IActorRefFactory factory, Action<string> listner,
            Action<IOperationResult> onCompled, string? name = null)
            => CreateListner(factory, listner, onCompled, Timeout.InfiniteTimeSpan, name);

        public static IActorRef CreateListner(IActorRefFactory factory, Reporter reporter,
            Action<IOperationResult> onCompled, TimeSpan timeout, string? name = null)
            => CreateListner(factory, reporter.Send, onCompled, timeout, name);

        public static IActorRef CreateListner(IActorRefFactory factory, Action<string> listner,
            TaskCompletionSource<IOperationResult> onCompled, TimeSpan timeout, string? name = null)
            => CreateListner(factory, listner, onCompled.SetResult, timeout, name);

        public static IActorRef CreateListner(IActorRefFactory factory, Action<string> listner,
            TaskCompletionSource<IOperationResult> onCompled, string? name = null)
            => CreateListner(factory, listner, onCompled, Timeout.InfiniteTimeSpan, name);

        public static IActorRef CreateListner(IActorRefFactory factory, Reporter reporter,
            TaskCompletionSource<IOperationResult> onCompled, TimeSpan timeout, string? name = null)
            => CreateListner(factory, reporter.Send, onCompled, timeout, name);

        public static IActorRef CreateListner(IActorRefFactory factory, Action<string> listner, TimeSpan timeout,
            string? name, Action<Task<IOperationResult>> onCompled)
        {
            var source = new TaskCompletionSource<IOperationResult>();
            var actor = CreateListner(factory, listner, source, timeout, name);
            onCompled(source.Task);
            return actor;
        }

        public static IActorRef CreateListner(IActorRefFactory factory, Action<string> listner, string name,
            Action<Task<IOperationResult>> onCompled)
            => CreateListner(factory, listner, Timeout.InfiniteTimeSpan, name, onCompled);

        public static IActorRef CreateListner(IActorRefFactory factory, Reporter reporter, TimeSpan timeout,
            string? name, Action<Task<IOperationResult>> onCompled)
            => CreateListner(factory, reporter.Send, timeout, name, onCompled);

        public static IActorRef CreateListner(IActorRefFactory factory, Action<string> listner, TimeSpan timeout,
            Action<Task<IOperationResult>> onCompled)
            => CreateListner(factory, listner, timeout, null, onCompled);

        public static IActorRef CreateListner(IActorRefFactory factory, Action<string> listner,
            Action<Task<IOperationResult>> onCompled)
            => CreateListner(factory, listner, Timeout.InfiniteTimeSpan, null, onCompled);

        public static IActorRef CreateListner(IActorRefFactory factory, Reporter reporter, TimeSpan timeout,
            Action<Task<IOperationResult>> onCompled)
            => CreateListner(factory, reporter.Send, timeout, null, onCompled);

        public static IActorRef CreateListner(IActorRefFactory factory, Action<string> listner, TimeSpan timeout,
            string? name, out Task<IOperationResult> onCompled)
        {
            var source = new TaskCompletionSource<IOperationResult>();
            var actor = CreateListner(factory, listner, source, timeout, name);
            onCompled = source.Task;
            return actor;
        }

        public static IActorRef CreateListner(IActorRefFactory factory, Action<string> listner, TimeSpan timeout,
            out Task<IOperationResult> onCompled)
            => CreateListner(factory, listner, timeout, null, out onCompled);

        public Reporter Listen(IActorRef actor)
        {
            if (_reporter.IsNobody()) return this;

            if (_compledCalled.Value)
                throw new InvalidOperationException("Reporter is Compled");
            _reporter.Tell(new ListeningActor(actor));

            return this;
        }

        public void Send(string message)
        {
            if (_reporter.IsNobody()) return;

            if (_compledCalled.Value)
                throw new InvalidOperationException("Reporter is Compled");
            _reporter.Tell(new TransferedMessage(message));
        }

        public void Compled(IOperationResult result)
        {
            if(_reporter.IsNobody()) return;

            if (_compledCalled.GetAndSet(true))
                throw new InvalidOperationException("Reporter is Compled");

            _reporter.Tell(result);
        }

        private sealed class Listner : ReceiveActor
        {
            private bool _compled;

            public Listner(Action<string> listner, Action<IOperationResult> onCompled, TimeSpan timeSpan)
            {
                Receive<IOperationResult>(c =>
                                          {
                                              if (_compled) return;
                                              _compled = true;
                                              Context.Stop(Self);
                                              onCompled(c);
                                          });
                Receive<TransferedMessage>(m => listner(m.Message));

                if (timeSpan == Timeout.InfiniteTimeSpan)
                    return;

                Task.Delay(timeSpan).PipeTo(Self, success: () => OperationResult.Failure(new Error(TimeoutError, TimeoutError)));
                //Timers.StartSingleTimer(timeSpan, OperationResult.Failure(new Error(TimeoutError, TimeoutError)), timeSpan);
            }
        }

        private sealed class ReporterActor : ReceiveActor
        {
            private readonly List<IActorRef> _listner = new();

            public ReporterActor()
            {
                Receive<TransferedMessage>(msg =>
                {
                    foreach (var actorRef in _listner) actorRef.Forward(msg);
                });

                Receive<IOperationResult>(msg =>
                {
                    foreach (var actorRef in _listner) actorRef.Forward(msg);
                    Context.Stop(Self);
                });

                Receive<ListeningActor>(a =>
                {
                    Context.Watch(a.Actor);
                    _listner.Add(a.Actor);
                });

                Receive<Terminated>(t => { _listner.Remove(t.ActorRef); });
            }
        }

        private sealed record ListeningActor(IActorRef Actor);

        private sealed record TransferedMessage(string Message);
    }

    public static class ReporterExtensions
    {
        public static IObservable<ReporterEvent<TMessage, TState>> Report<TMessage, TState>(this IObservable<ReporterEvent<TMessage, TState>> input, string message)
            where TMessage : IReporterMessage
            => input.Select(i =>
                            {
                                i.Reporter.Send(message);
                                return i;
                            });

        public static IObservable<ReporterEvent<TMessage, TState>> Report<TMessage, TState>(this IObservable<ReporterEvent<TMessage, TState>> input, Func<string> message)
            where TMessage : IReporterMessage
            => input.Select(i =>
                            {
                                i.Reporter.Send(message());
                                return i;
                            });

        public static IObservable<ReporterEvent<TMessage, TState>> Report<TMessage, TState>(this IObservable<ReporterEvent<TMessage, TState>> input, Func<TMessage, string> message)
            where TMessage : IReporterMessage
            => input.Select(i =>
                            {
                                i.Reporter.Send(message(i.Event));
                                return i;
                            });
    }
}