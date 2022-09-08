using System.Collections.Concurrent;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Tauron.Applicarion.Redux.Internal.Configuration;

namespace Tauron.Applicarion.Redux.Internal;

public sealed class RootStore : IRootStore
{
    private readonly Dictionary<Type, IActionDispatcher> _stores = new();
    private readonly Dictionary<Type, IMiddleware> _middlewares = new();
    private readonly Subject<object> _actions = new();
    private readonly IDisposable _subscription;
    
    public bool CanProcess<TAction>()
        => _stores.Values.Any(d => d.CanProcess<TAction>());

    public IObservable<TAction> ObservAction<TAction>() where TAction : class
        => _actions.OfType<TAction>();

    public void Dispatch(object action)
        => _actions.OnNext(action);

    public void Dispose()
    {
        _actions.Dispose();
        _subscription.Dispose();
        _stores.Clear();
    }

    public IObservable<object> ObserveActions()
        => _actions.AsObservable();

    public IRootStoreState<TState> ForState<TState>() where TState : new()
    {
        if(_stores.TryGetValue(typeof(TState), out var store) && store is IReduxStore<TState> reduxStore)
            return new RootStoreState<TState>(reduxStore)
    }

    public void RegisterMiddlewares(IEnumerable<IMiddleware> middlewares)
    {
        throw new NotImplementedException();
    }

    public TMiddleware GetMiddleware<TMiddleware>(Func<TMiddleware> factory) where TMiddleware : IMiddleware
        => throw new NotImplementedException();

    public void RegisterMiddlewares(params IMiddleware[] middlewares)
    {
        throw new NotImplementedException();
    }
}