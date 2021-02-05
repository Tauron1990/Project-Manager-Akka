using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Akka.Actor;
using JetBrains.Annotations;
using Tauron.Application.Workshop.Core;
using Tauron.Application.Workshop.Mutating;
using Tauron.Application.Workshop.Mutating.Changes;

namespace Tauron.Application.Workshop.Mutation
{
    [PublicAPI]
    public sealed class MutatingEngine<TData> : MutatingEngine, IEventSourceable<TData>
        where TData : class
    {
        private readonly IDataSource<TData> _dataSource;
        private readonly Task<IActorRef> _mutator;
        private readonly Subject<TData> _responder;
        private readonly WorkspaceSuperviser _superviser;

        internal MutatingEngine(Task<IActorRef> mutator, IDataSource<TData> dataSource, WorkspaceSuperviser superviser)
            : base(mutator, superviser)
        {
            _mutator = mutator;
            _dataSource = dataSource;
            _superviser = superviser;
            _responder = new Subject<TData>();
            _responder.Subscribe(dataSource.SetData);
        }

        public MutatingEngine(IDataSource<TData> dataSource) : base(Task.FromResult<IActorRef>(ActorRefs.Nobody),
            new WorkspaceSuperviser())
        {
            _mutator = Task.FromResult<IActorRef>(ActorRefs.Nobody);
            _dataSource = dataSource;
            _superviser = new WorkspaceSuperviser();
            _responder = new Subject<TData>();
            _responder.Subscribe(_dataSource.SetData);
        }

        //public IDataMutation CreateMutate(string name, Func<TData, Task<TData?>> transform, object? hash = null)
        //{
        //    async Task Runner()
        //    {
        //        var subject = new Subject<TData>();

        //        _responder.Push(await transform(_dataSource.GetData()));
        //    }

        //    return new AsyncDataMutation<TData>(Runner, name, hash);
        //}

        public IEventSource<TRespond> EventSource<TRespond>(Func<TData, TRespond> transformer,
            Func<TData, bool>? where = null)
            => new EventSource<TRespond, TData>(_superviser, _mutator, transformer, where, _responder);

        public IEventSource<TRespond> EventSource<TRespond>(Func<IObservable<TData>, IObservable<TRespond>> transform)
            => new EventSource<TRespond, TData>(_superviser, _mutator, transform(_responder.AsObservable()));


        public void Mutate(string name, Func<IObservable<TData>, IObservable<TData?>> transform, object? hash = null)
        {
            Mutate(CreateMutate(name, transform, hash));
        }

        public IDataMutation CreateMutate(string name, Func<IObservable<TData>, IObservable<TData?>> transform,
            object? hash = null)
        {
            void Runner()
            {
                using var sender = new Subject<TData>();

                transform(sender.AsObservable()).NotNull().Subscribe(m => _responder.OnNext(m));
                sender.OnNext(_dataSource.GetData());
                sender.OnCompleted();
                sender.Dispose();
            }

            return new DataMutation<TData>(Runner, name, hash);
        }
    }

    [PublicAPI]
    public sealed class ExtendedMutatingEngine<TData> : MutatingEngine, IEventSourceable<TData>
        where TData : class
    {
        private readonly IExtendedDataSource<TData> _dataSource;
        private readonly Task<IActorRef> _mutator;
        private readonly ResponderList _responder;
        private readonly WorkspaceSuperviser _superviser;

        internal ExtendedMutatingEngine(Task<IActorRef> mutator, IExtendedDataSource<TData> dataSource,
            WorkspaceSuperviser superviser)
            : base(mutator, superviser)
        {
            _mutator = mutator;
            _dataSource = dataSource;
            _superviser = superviser;
            _responder = new ResponderList(_dataSource.SetData, dataSource.OnCompled);
        }

        public IEventSource<TRespond> EventSource<TRespond>(Func<TData, TRespond> transformer,
            Func<TData, bool>? where = null)
            => new EventSource<TRespond, TData>(_superviser, _mutator, transformer, where, _responder);

        public IEventSource<TRespond> EventSource<TRespond>(Func<IObservable<TData>, IObservable<TRespond>> transform)
            => new EventSource<TRespond, TData>(_superviser, _mutator, transform(_responder));

        public void Mutate(string name, IQuery query, Func<IObservable<TData>, IObservable<TData?>> transform)
        {
            Mutate(CreateMutate(name, query, transform));
        }

        public IDataMutation CreateMutate(string name, IQuery query,
            Func<IObservable<TData>, IObservable<TData?>> transform)
        {
            async Task Runner()
            {
                var sender = new Subject<TData>();
                _responder.Push(query, transform(sender).NotNull());

                var data = await _dataSource.GetData(query);

                sender.OnNext(data);
                sender.OnCompleted();
            }

            return new AsyncDataMutation<TData>(Runner, name, query.ToHash());
        }

        private sealed class ResponderList : IObservable<TData>
        {
            private readonly Func<IQuery, Task> _completer;
            private readonly Subject<TData> _handler = new();
            private readonly Func<IQuery, TData, Task> _root;

            public ResponderList(Func<IQuery, TData, Task> root, Func<IQuery, Task> completer)
            {
                _root = root;
                _completer = completer;
            }

            public IDisposable Subscribe(IObserver<TData> observer) => _handler.Subscribe(observer);

            public void Push(IQuery query, IObservable<TData> dataFunc)
            {
                dataFunc
                    .SelectMany(async data =>
                    {
                        try
                        {
                            await _root(query, data);
                            _handler.OnNext(data);
                        }
                        finally
                        {
                            await _completer(query);
                        }

                        return Unit.Default;
                    })
                    .Timeout(TimeSpan.FromMinutes(10))
                    .Subscribe();
            }
        }
    }

    [PublicAPI]
    public class MutatingEngine : DeferredActor
    {
        private readonly Task<IActorRef> _mutator;
        private readonly WorkspaceSuperviser _superviser;

        protected MutatingEngine(Task<IActorRef> mutator, WorkspaceSuperviser superviser)
            : base(mutator)
        {
            _mutator = mutator;
            _superviser = superviser;
        }

        public static MutatingEngine Create(WorkspaceSuperviser superviser, Func<Props, Props>? configurate = null)
        {
            var mutatorProps = Props.Create<MutationActor>();
            mutatorProps = configurate?.Invoke(mutatorProps) ?? mutatorProps;

            var mutator = superviser.Create(mutatorProps, "Mutator");
            return new MutatingEngine(mutator, superviser);
        }

        public static ExtendedMutatingEngine<TData> From<TData>(IExtendedDataSource<TData> source,
            WorkspaceSuperviser superviser, Func<Props, Props>? configurate = null)
            where TData : class
        {
            var mutatorProps = Props.Create<MutationActor>();
            mutatorProps = configurate?.Invoke(mutatorProps) ?? mutatorProps;

            var mutator = superviser.Create(mutatorProps, "Mutator");
            return new ExtendedMutatingEngine<TData>(mutator, source, superviser);
        }

        public static ExtendedMutatingEngine<TData> From<TData>(IExtendedDataSource<TData> source,
            MutatingEngine parent)
            where TData : class
            => new(parent._mutator, source, parent._superviser);

        public static MutatingEngine<TData> From<TData>(IDataSource<TData> source, WorkspaceSuperviser superviser,
            Func<Props, Props>? configurate = null)
            where TData : class
        {
            var mutatorProps = Props.Create<MutationActor>();
            mutatorProps = configurate?.Invoke(mutatorProps) ?? mutatorProps;

            var mutator = superviser.Create(mutatorProps, "Mutator");
            return new MutatingEngine<TData>(mutator, source, superviser);
        }

        public static MutatingEngine<TData> From<TData>(IDataSource<TData> source, MutatingEngine parent)
            where TData : class
            => new(parent._mutator, source, parent._superviser);

        public static MutatingEngine<TData> Dummy<TData>(IDataSource<TData> source)
            where TData : class
            => new(source);

        public void Mutate(IDataMutation mutationOld)
        {
            TellToActor(mutationOld);
        }
    }

    [PublicAPI]
    public static class MutatinEngineExtensions
    {
        public static IEventSource<TEvent> EventSource<TData, TEvent>(
            this IEventSourceable<MutatingContext<TData>> engine)
            where TEvent : MutatingChange
        {
            return engine.EventSource(c => c.GetChange<TEvent>(), c => c.Change?.Cast<TEvent>() != null);
        }
    }
}