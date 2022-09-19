using Akka;
using Akka.Streams;
using Akka.Streams.Dsl;
using Akka.Streams.Stage;
using JetBrains.Annotations;
using Stl.Fusion;
using Tauron.Application.Akka.Redux.Extensions.Internal;

namespace Tauron.Application.Akka.Redux.Extensions;

[PublicAPI]
public static class DynamicUpdate
{
    internal static Source<TData, NotUsed> ToSource<TData>(IState<TData> state, bool skipErrors = false)
        => Source.FromGraph(GraphDsl.Create(b => b.Add(new StatePusher<TData>(state, skipErrors))));
    
    private sealed class StatePusher<TData> : GraphStage<SourceShape<TData>>
    {
        private sealed class Logic : GraphStageLogic
        {
            private readonly Outlet<TData> _out;
            private readonly IState<TData> _state;
            private readonly bool _skipErrors;

            public Logic(StatePusher<TData> pusher) : base(pusher.Shape)
            {
                _out = pusher.Output;
                _state = pusher._state;
                _skipErrors = pusher._skipErrors;
                
                _state.AddEventHandler(StateEventKind.Updated | StateEventKind.Updating, NewData);
            }

            private void NewData(IState<TData> state, StateEventKind kind)
            {
                SetHandler(_out, DoNothing);
                
                if(state.HasError && !_skipErrors)
                    FailStage(state.Error);
                else if(kind == StateEventKind.Updated && state.HasValue)
                {
                    if(IsAvailable(_out))
                        PushNext();
                    else
                        SetHandler(_out, PushNext);
                }
            }

            private void PushNext()
            {
                try
                {
                    SetHandler(_out, DoNothing);
                    Push(_out, _state.Value);
                }
                catch (Exception e)
                {
                    FailStage(e);
                }
            }
        }
        
        private readonly IState<TData> _state;
        private readonly bool _skipErrors;

        public StatePusher(IState<TData> state, bool skipErrors)
        {
            _state = state;
            _skipErrors = skipErrors;
            Shape = new SourceShape<TData>(Output);
        }

        private Outlet<TData> Output { get; } = new("StatePusher.out");

        public override SourceShape<TData> Shape { get; }
        
        protected override GraphStageLogic CreateLogic(Attributes inheritedAttributes)
            => new Logic(this);
    }


    private static Effect<TState> CreateDynamicUpdaterInternal<TState, TSource, TData>(
        IStateFactory stateFactory,
        Selector<TState, TSource> selector,
        Func<CancellationToken, Func<CancellationToken, ValueTask<TSource>>, Task<TData>> requester,
        Patcher<TData, TState> patcher)
        => Create.Effect<TState>(
            store =>
            {
                var stlState = stateFactory.NewMutable<TSource>();
                store.Select(Flow.Create<TState>().Select(t => selector(t)))
                   .RunForeach(data => stlState.Set(data), store.Materializer);

                var computer = stateFactory.NewComputed(
                    new ComputedState<TData>.Options(),
                    async (_, token) => await requester(token, stlState.Use).ConfigureAwait(false));

                return ToSource(computer, true)
                   .Select(data => patcher(data, store.CurrentState))
                   .Select(data => (object)MutateCallback.Create(data))!;
            }
        );

    public static void AddRequest<TState, TAction>(
        IRootStore store,
        Func<TAction, ValueTask<string?>> runRequest, Func<TState, TAction, TState> onScess)
        where TAction : class where TState : new()
        => AsyncRequestMiddleware
           .Get<TState>(store)
           .AddRequest(runRequest, onScess, (state, _) => state);

    public static void AddRequest<TState, TAction>(
        IRootStore store,
        Func<TAction, ValueTask<string?>> runRequest,
        Func<TState, TAction, TState> onScess,
        Func<TState, object, TState> onFail)
        where TAction : class where TState : new()
        => AsyncRequestMiddleware
           .Get<TState>(store)
           .AddRequest(runRequest, onScess, onFail);

    public static void AddRequest<TState, TAction>(IRootStore store, Func<TAction, Task<string?>> runRequest, Func<TState, TAction, TState> onScess)
        where TAction : class where TState : new()
        => AsyncRequestMiddleware
           .Get<TState>(store)
           .AddRequest(runRequest, onScess, (state, _) => state);

    public static void AddRequest<TState, TAction>(
        IRootStore store,
        Func<TAction, Task<string?>> runRequest,
        Func<TState, TAction, TState> onScess,
        Func<TState, object, TState> onFail)
        where TAction : class where TState : new()
        => AsyncRequestMiddleware
           .Get<TState>(store)
           .AddRequest(runRequest, onScess, onFail);

    public static void OnTheFlyUpdate<TState, TSource, TData>(
        IReduxStore<TState> store,
        IStateFactory stateFactory,
        Selector<TState, TSource> sourceSelector,
        Func<CancellationToken, Func<CancellationToken, ValueTask<TSource>>, Task<TData>> fetcher,
        Patcher<TData, TState> patcher)
        => store.RegisterEffects(CreateDynamicUpdaterInternal(stateFactory, sourceSelector, fetcher, patcher));

    public static void OnTheFlyUpdate<TState, TData>(
        IReduxStore<TState> store,
        IStateFactory stateFactory,
        Func<CancellationToken, Task<TData>> fetcher,
        Patcher<TData, TState> patcher)
        => store.RegisterEffects(
            CreateDynamicUpdaterInternal(
                stateFactory,
                _ => 0,
                async (token, sel) =>
                {
                    await sel(token).ConfigureAwait(false);

                    return await fetcher(token).ConfigureAwait(false);
                },
                patcher));
}