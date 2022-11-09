using System.Reactive.Linq;
using JetBrains.Annotations;

namespace Tauron.Applicarion.Redux.Internal;

[PublicAPI]
public abstract class Middleware : IMiddleware
{
    private readonly List<FilterRegistration> _processors = new();
    private IRootStore? _store;

    protected IRootStore Store
    {
        get
        {
            if(_store is null)
                throw new InvalidOperationException("Middleware not Initalized");

            return _store;
        }
    }

    public virtual void Init(IRootStore rootStore)
        => _store = rootStore;

    public IObservable<object> Connect(IRootStore actionObservable)
    {
        return actionObservable
           .ObserveActions()
           .Where(_ => _processors.Count != 0)
           .SelectMany(
                dispatchedAction => _processors.FirstOrDefault(r => r.Can(dispatchedAction))?.Exec(dispatchedAction) ??
                                    Observable.Empty<object>());
    }

    protected void OnAction<TAction>(Func<IObservable<TAction>, IObservable<object>> runner)
    {
        _processors.Add(
            new FilterRegistration(
                a => a is TAction,
                o => runner(
                    from a in o
                    where a is TAction
                    select (TAction)a)));
    }

    private delegate IObservable<object> DispatchNextInternal(IObservable<object> action);

    private sealed class FilterRegistration
    {
        private readonly Predicate<object> _filter;
        private readonly DispatchNextInternal _runner;

        public FilterRegistration(Predicate<object> filter, DispatchNextInternal runner)
        {
            _filter = filter;
            _runner = runner;
        }

        public bool Can(object action)
            => _filter(action);

        public IObservable<object> Exec(object action)
            => _runner(Observable.Return(action));
    }
}