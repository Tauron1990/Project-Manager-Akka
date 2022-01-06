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

    public IObservable<TState> Connect(IObservable<DispatchedAction<TState>> actionObservable, DispatchNext<TState> next)
    {
        Initialize();

        return next(actionObservable
           .SelectMany(
                dispatchedAction =>
                {
                    return
                        _processors.FirstOrDefault(r => r.Can(dispatchedAction))?.Exec(dispatchedAction) ??
                        Observable.Return(dispatchedAction);
                }));
    }

    protected void Dispatch(object action)
        => Store.Dispatch(action);

    protected abstract void Initialize();
    
    protected void OnAction<TAction>(Func<IObservable<DispatchedAction<TState>>, IObservable<DispatchedAction<TState>>> runner)
    
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