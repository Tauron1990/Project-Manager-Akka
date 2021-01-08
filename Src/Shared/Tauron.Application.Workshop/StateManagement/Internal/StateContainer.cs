using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Tauron.Application.Workshop.Mutating;
using Tauron.Application.Workshop.Mutation;
using Tauron.ObservableExt;

namespace Tauron.Application.Workshop.StateManagement.Internal
{
    public abstract class StateContainer : IDisposable
    {
        protected StateContainer(IState instance) => Instance = instance;

        public IState Instance { get; }
        public abstract void Dispose();

        public abstract IDataMutation? TryDipatch(IStateAction action, IObserver<IReducerResult> sendResult, IObserver<Unit> onCompled);
    }

    public sealed class StateContainer<TData> : StateContainer
        where TData : class
    {
        private readonly IDisposable _toDispose;

        public StateContainer(IState instance, IReadOnlyCollection<IReducer<TData>> reducers, ExtendedMutatingEngine<MutatingContext<TData>> mutatingEngine, IDisposable toDispose)
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
                                                                             Observable.Return(d).Select(dd => ReducerResult.Sucess(dd) with {StartLine = true}).SingleAsync(),
                                                                             (observable, reducerBuilder) =>
                                                                             {
                                                                                 return observable
                                                                                       .ConditionalSelect()
                                                                                       .ToSame(b =>
                                                                                               {
                                                                                                   b.When(r => !r.IsOk || r.Data == null, o => o.Do(_ => cancel.OnNext(Unit.Default)))
                                                                                                    .When(r => r.IsOk && r.Data != null,
                                                                                                          o => reducerBuilder(o.Select(d => d.Data!)));
                                                                                               });
                                                                             })).Switch().Publish().RefCount();

                                     subs.Add(processor.Select(_ => Unit.Default).Subscribe(onCompled));
                                     subs.Add(processor.Cast<IReducerResult>().Subscribe(sendResult));
                                     subs.Add(processor.Subscribe(_ => { }, () => subs.Dispose()));

                                     return processor.Where(r => r.IsOk && !r.StartLine).Select(r => r.Data);
                                 }
                                 catch
                                 {
                                     onCompled.OnCompleted();
                                     throw;
                                 }
                             });
        }

        public override void Dispose()
        {
            _toDispose.Dispose();
        }
    }
}