using JetBrains.Annotations;
using ReduxSimple;
using Tauron.Application;
using Tauron.Application.Blazor;

namespace SimpleProjectManager.Client.Data.StateContainer;

[PublicAPI]
public abstract class ContainerBase<TState> 
    where TState : class, new()
{
    public IEventAggregator Aggregator { get; }
    
    protected ReduxStore<TState> Store { get; }

    protected Factory C { get; }
    
    protected ContainerBase(IEventAggregator aggregator)
    {
        Aggregator = aggregator;
        C = new Factory(aggregator);
        // ReSharper disable VirtualMemberCallInConstructor
        Store = new ReduxStore<TState>(CreateReducers()); 
        Store.RegisterEffects(CreateEffects().ToArray());
        InitStore();
    }

    protected abstract IEnumerable<On<TState>> CreateReducers();

    protected virtual IEnumerable<Effect<TState>> CreateEffects()
    {
        yield break;
    }

    protected abstract void InitStore();

    [PublicAPI]
    protected class Factory
    {
        private readonly IEventAggregator _aggregator;

        public Factory(IEventAggregator aggregator)
            => _aggregator = aggregator;

        public IEnumerable<On<TState>> CombineReducers(params IEnumerable<On<TState>>[] reducersList)
            => Reducers.CombineReducers(reducersList);

        public On<TState> On<TAction>(Func<TState, TAction, TState> reducer) 
            where TAction : class
        {
            TState Exec(TState state, TAction action)
            {
                try
                {
                    return reducer(state, action);
                }
                catch (Exception e)
                {
                    _aggregator.PublishError(e);

                    return state;
                }
            }
            
            return Reducers.On<TAction, TState>(Exec);
        }

        public On<TState> On<TAction>(Func<TState, TState> reducer) 
            where TAction : class
        {
            TState Exec(TState state)
            {
                try
                {
                    return reducer(state);
                }
                catch (Exception e)
                {
                    _aggregator.PublishError(e);

                    return state;
                }
            }
            
            return Reducers.On<TAction, TState>(Exec);
        }

        public Effect<TState> CreateEffect(Func<ReduxStore<TState>, IObservable<object>> run, bool dispatch = true)
            => Effects.CreateEffect(run, dispatch);
        
        public Effect<TState> CreateEffect(Func<IObservable<object>> run, bool dispatch = true)
            => Effects.CreateEffect<TState>(run, dispatch);
    }
}