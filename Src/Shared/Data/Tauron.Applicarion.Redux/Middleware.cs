using System.Reactive.Linq;
using JetBrains.Annotations;

namespace Tauron.Applicarion.Redux;

[PublicAPI]
public abstract class Middleware<TState> : IMiddleware<TState>
{
    private readonly List<FilterRegistration> _processors = new();

    protected IStore<TState> Store { get; private set; } = null!;
    
    public virtual void Initialize(IStore<TState> store)
        => Store = store;

    public IObservable<DispatchedAction<TState>> Connect(IObservable<DispatchedAction<TState>> actionObservable, DispatchNext<TState> next)
        => next(actionObservable
           .SelectMany(
                dispatchedAction =>
                {
                    return
                        _processors.FirstOrDefault(r => r.Can(dispatchedAction))?.Exec(dispatchedAction) ??
                        Observable.Return(dispatchedAction);
                }));

    protected void Dispatch(object action)
        => Store.Dispatch(action);

    protected void OnAction<TAction>(Func<IObservable<TypedDispatechedAction<TAction>>, IObservable<DispatchedAction<TState>>> runner)
    {
        _processors.Add(new FilterRegistration(
            a => a.Action is TAction,
            o => runner(from da1 in o
                        where da1.Action is TAction
                        select new TypedDispatechedAction<TAction>(da1.State, ((TAction)da1.Action!)!))));
    }

    [PublicAPI]
    protected sealed record TypedDispatechedAction<TAction>(TState State, TAction Action)
    {
        public DispatchedAction<TState> NewAction(object action) => new(State, action);
    }
    
    private delegate IObservable<DispatchedAction<TState>> DispatchNextInternal(IObservable<DispatchedAction<TState>> action);
    
    private sealed class FilterRegistration
    {
        private readonly Predicate<DispatchedAction<TState>> _filter;
        private readonly DispatchNextInternal _runner;

        public FilterRegistration(Predicate<DispatchedAction<TState>> filter, DispatchNextInternal runner)
        {
            _filter = filter;
            _runner = runner;
        }

        public bool Can(DispatchedAction<TState> action)
            => _filter(action);

        public IObservable<DispatchedAction<TState>> Exec(DispatchedAction<TState> action)
            => _runner(Observable.Return(action));
    }
}