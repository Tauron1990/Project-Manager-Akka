using System.Reactive.Linq;
using System.Reactive.Subjects;
using Tauron.Applicarion.Redux.Internal;
using Tauron.Operations;

namespace Tauron.Applicarion.Redux.Extensions.Internal;

public static class AsyncRequestMiddleware
{
    public static AsyncRequestMiddleware<TState> Get<TState>(IRootStore store) where TState : new()
        => store.GetMiddleware(AsyncRequestMiddleware<TState>.Factory);
}

public sealed class AsyncRequestMiddleware<TState> : Middleware where TState : new()
{
    internal static readonly Func<AsyncRequestMiddleware<TState>> Factory = () => new AsyncRequestMiddleware<TState>();

    private readonly BehaviorSubject<TState> _currentState = new(new TState());

    public override void Init(IRootStore rootStore)
    {
        base.Init(rootStore);
        rootStore.ForState<TState>().Select().Subscribe(_currentState);
    }

    public void AddRequest<TAction>(
        Func<TAction, Task<SimpleResult>> runRequest,
        Func<TState, TAction, TState> onScess,
        Func<TState, object, TState> onFail)
        where TAction : class
    {
        IObservable<object> Runner(IObservable<TAction> arg)
            => arg.SelectManySafe(async action => (action, result: await runRequest(action).ConfigureAwait(false)))
               .ConvertResult(
                    pair => pair.result.IsSuccess() ? onScess(_currentState.Value, pair.action) : onFail(_currentState.Value, pair.result),
                    exception => onFail(_currentState.Value, exception))
               .Select(MutateCallback.Create);

        OnAction<TAction>(Runner);
    }

    public void AddRequest<TAction>(
        Func<TAction, ValueTask<SimpleResult>> runRequest,
        Func<TState, TAction, TState> onScess,
        Func<TState, object, TState> onFail)
        where TAction : class
    {
        IObservable<object> Runner(IObservable<TAction> arg)
            => arg.SelectManySafe(async action => (Action: action, result: await runRequest(action).ConfigureAwait(false)))
               .ConvertResult(
                    pair => pair.result.IsSuccess() ? onScess(_currentState.Value, pair.Action) : onFail(_currentState.Value, pair.result),
                    exception => onFail(_currentState.Value, exception))
               .Select(MutateCallback.Create);

        OnAction<TAction>(Runner);
    }
}