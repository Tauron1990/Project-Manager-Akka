using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Util;
using Tauron.Application.Workshop.Mutating;
using Tauron.Application.Workshop.Mutation;
using Tauron.Application.Workshop.StateManagement.StatePooling;
using Tauron.ObservableExt;

namespace Tauron.Application.Workshop.StateManagement.Internal
{
    public abstract class StateContainer : IDisposable
    {
        protected StateContainer(IStateInstance instance) => Instance = instance;

        public IStateInstance Instance { get; }
        public abstract void Dispose();

        public abstract IDataMutation? TryDipatch(IStateAction action, IObserver<IReducerResult> sendResult, IObserver<Unit> onCompled);
    }

    public interface IStateInstance
    {
        void InitState<TData>(ExtendedMutatingEngine<MutatingContext<TData>> engine);
        void ApplyQuery<TData>(IExtendedDataSource<MutatingContext<TData>> engine)
            where TData : class, IStateEntity;

    }

    public sealed class PhysicalInstance : IStateInstance
    {
        public IState State { get; }

        public PhysicalInstance(IState state) => State = state;

        public void InitState<TData>(ExtendedMutatingEngine<MutatingContext<TData>> engine)
        {
            if(State is IInitState<TData> init)
                init.Init(engine);
        }

        public void ApplyQuery<TData>(IExtendedDataSource<MutatingContext<TData>> engine) where TData : class, IStateEntity
        {
            if(State is ICanQuery<TData> canQuery)
                canQuery.DataSource(engine);
        }
    }

    public sealed class ActorRefInstance : IStateInstance
    {
        public Task<IActorRef> ActorRef { get; }
        private readonly Type _targetType;

        public ActorRefInstance(Task<IActorRef> actorRef, Type targetType)
        {
            ActorRef = actorRef;
            _targetType = targetType;
        }


        public void InitState<TData>(ExtendedMutatingEngine<MutatingContext<TData>> engine)
        {
            if(_targetType.Implements<IInitState<TData>>())
                ActorRef.ToObservable().Subscribe(a => a.Tell(StateActorMessage.Create(engine)));
        }

        public void ApplyQuery<TData>(IExtendedDataSource<MutatingContext<TData>> engine) where TData : class, IStateEntity
        {
            if(_targetType.Implements<ICanQuery<TData>>())
                ActorRef.ToObservable().Subscribe(m => m.Tell(StateActorMessage.Create(engine)));
        }
    }

    public abstract class StateActorMessage
    {
        private sealed class InitMessage<TData> : StateActorMessage
        {
            private readonly ExtendedMutatingEngine<MutatingContext<TData>> _engine;

            public InitMessage(ExtendedMutatingEngine<MutatingContext<TData>> engine) => _engine = engine;
            public override void Apply(object @this)
            {
                if(@this is IInitState<TData> state)
                    state.Init(_engine);
            }
        }

        private sealed class QueryMessage<TData> : StateActorMessage 
            where TData : class, IStateEntity
        {
            private readonly IExtendedDataSource<MutatingContext<TData>> _dataSource;

            public QueryMessage(IExtendedDataSource<MutatingContext<TData>> dataSource) => _dataSource = dataSource;
            public override void Apply(object @this)
            {
                if(@this is ICanQuery<TData> query)
                    query.DataSource(_dataSource);
            }
        }

        public static StateActorMessage Create<TData>(IExtendedDataSource<MutatingContext<TData>> source) 
            where TData : class, IStateEntity => new QueryMessage<TData>(source);

        public static StateActorMessage Create<TData>(ExtendedMutatingEngine<MutatingContext<TData>> engine)
            => new InitMessage<TData>(engine);

        public abstract void Apply(object @this);
    }

    public sealed class StateContainer<TData> : StateContainer
        where TData : class
    {
        private readonly IDisposable _toDispose;

        public StateContainer(IStateInstance instance, IReadOnlyCollection<IReducer<TData>> reducers, ExtendedMutatingEngine<MutatingContext<TData>> mutatingEngine, IDisposable toDispose)
            : base(instance)
        {
            _toDispose = toDispose;
            Reducers = reducers;
            MutatingEngine = mutatingEngine;
        }

        private IReadOnlyCollection<IReducer<TData>> Reducers { get; }
        private ExtendedMutatingEngine<MutatingContext<TData>> MutatingEngine { get; }

        public override IDataMutation? TryDipatch(IStateAction action, IObserver<IReducerResult> sendResult, IObserver<Unit> onCompled)
        {
            var reducers = Reducers.Where(r => r.ShouldReduceStateForAction(action)).ToList();
            if (reducers.Count == 0)
                return null;

            return MutatingEngine
                .CreateMutate(action.ActionName, action.Query,
                    data =>
                    {
                        try
                        {
                            var subs = new CompositeDisposable(3);
                            var cancel = new Subject<Unit>();
                            subs.Add(cancel);

                            var reducer =
                                (from reducerFactory in reducers.ToObservable()
                                    where reducerFactory.ShouldReduceStateForAction(action)
                                    select reducerFactory.Reduce(action)).TakeUntil(cancel);

                            var processor = data.SelectMany(d =>
                                reducer.Aggregate(
                                    Observable.Return(d).Select(dd => ReducerResult.Sucess(dd) with {StartLine = true})
                                        .SingleAsync(),
                                    (observable, reducerBuilder) =>
                                    {
                                        return observable
                                            .ConditionalSelect()
                                            .ToSame(b =>
                                            {
                                                b.When(r => !r.IsOk || r.Data == null,
                                                        o => o.Do(_ => cancel.OnNext(Unit.Default)))
                                                    .When(r => r.IsOk && r.Data != null,
                                                        o => reducerBuilder(o.Select(d => d.Data!)));
                                            });
                                    })).Switch().Publish().RefCount();

                            subs.Add(processor.Cast<IReducerResult>().Subscribe(sendResult));
                            subs.Add(processor.Subscribe(_ => { }, () => subs.Dispose()));

                            return processor.Where(r => r.IsOk && !r.StartLine).Select(r => r.Data);
                        }
                        catch
                        {
                            onCompled.OnCompleted();
                            throw;
                        }
                    }, onCompled);
        }

        public override void Dispose()
        {
            _toDispose.Dispose();
        }
    }
}