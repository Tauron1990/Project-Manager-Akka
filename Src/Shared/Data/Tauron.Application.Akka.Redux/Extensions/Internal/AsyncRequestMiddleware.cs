using Akka;
using Akka.Streams;
using Akka.Streams.Dsl;
using Tauron.Application.Akka.Redux.Internal;

namespace Tauron.Application.Akka.Redux.Extensions.Internal;

public static class AsyncRequestMiddleware
{
    public static AsyncRequestMiddleware<TState> Get<TState>(IRootStore store) where TState : new()
        => store.GetMiddleware(AsyncRequestMiddleware<TState>.Factory);
}

public sealed class AsyncRequestMiddleware<TState> : Middleware where TState : new()
{
    internal static readonly Func<AsyncRequestMiddleware<TState>> Factory = () => new AsyncRequestMiddleware<TState>();
    private IRootStoreState<TState>? _storeState;

    private IRootStoreState<TState> StoreState => Get(_storeState);

    private Flow<TAction, TransferedAction<TAction>, NotUsed> CreateStatefulFlow<TAction>()
        => Flow.FromGraph(
            GraphDsl.Create(
                b =>
                {
                    var combiner = b.Add(LastStateShape.Create<TState, TAction, TransferedAction<TAction>>((s, a) => new TransferedAction<TAction>(s, a)));

                    b.From(Source.Single(StoreState.CurrentState).Concat(StoreState.Select())).To(combiner.In0);

                    return new FlowShape<TAction, TransferedAction<TAction>>(combiner.In1, combiner.Out);
                }));

    public override void Init(IRootStore rootStore)
    {
        base.Init(rootStore);
        _storeState = rootStore.ForState<TState>();
    }

    public void AddRequest<TAction>(
        Func<TAction, Task<string?>> runRequest,
        Func<TState, TAction, TState> onScess,
        Func<TState, object, TState> onFail)
        where TAction : class
    {
        var flow = CreateStatefulFlow<TAction>()
           .SelectAsync(
                1,
                async transferedAction =>
                {
                    try
                    {
                        var result = await runRequest(transferedAction.Action);

                        return string.IsNullOrWhiteSpace(result) 
                            ? onScess(transferedAction.State, transferedAction.Action) 
                            : onFail(transferedAction.State, result);
                    }
                    catch (Exception e)
                    {
                        return onFail(transferedAction.State, e);
                    }
                })
           .Select(state => (object)MutateCallback.Create(state));
        
        OnAction(flow);
    }

    public void AddRequest<TAction>(
        Func<TAction, ValueTask<string?>> runRequest,
        Func<TState, TAction, TState> onScess,
        Func<TState, object, TState> onFail)
        where TAction : class
    {
        var flow = CreateStatefulFlow<TAction>()
           .SelectAsync(
                1,
                async transferedAction =>
                {
                    try
                    {
                        var result = await runRequest(transferedAction.Action);

                        return string.IsNullOrWhiteSpace(result) 
                            ? onScess(transferedAction.State, transferedAction.Action) 
                            : onFail(transferedAction.State, result);
                    }
                    catch (Exception e)
                    {
                        return onFail(transferedAction.State, e);
                    }
                })
           .Select(state => (object)MutateCallback.Create(state));
        
        OnAction(flow);
    }

    private sealed record TransferedAction<TAction>(TState State, TAction Action);
}