using Akka;
using Akka.Streams;
using Akka.Streams.Dsl;
using JetBrains.Annotations;

namespace Tauron.Application.Akka.Redux.Internal;

[PublicAPI]
public abstract class Middleware : IMiddleware
{
    private IRootStore? _store;
    private Source<object, NotUsed>? _actionsDispatcher;
    private Source<object, NotUsed>? _actions;
    private Sink<object, NotUsed>? _toDipatch;

    protected IRootStore Store => Get(_store);
    protected Source<object, NotUsed> Actions => Get(_actions);
    protected Sink<object, NotUsed> ToDispatch => Get(_toDipatch);

    protected TValue Get<TValue>(TValue? value)
    {
        if(value is null)
            throw new InvalidOperationException("Middleware not Initalized");
        return value;
    }
    
    public virtual void Init(IRootStore rootStore)
    {
        _store = rootStore;

        var (collector, source) = MergeHub.Source<object>().PreMaterialize(rootStore.Materializer);
        var (spreader, sink) = BroadcastHub.Sink<object>().PreMaterialize(rootStore.Materializer);

        _actions = spreader;
        _toDipatch = collector;
        _actionsDispatcher = source;
        
        rootStore.ObserveActions().RunWith(sink, rootStore.Materializer);
    }

    public Source<object, NotUsed> Connect(IRootStore actionObservable)
        => Get(_actionsDispatcher);

    protected void OnAction<TAction>(Flow<TAction, object, NotUsed> runner)
    {
        (from action in Actions
         where action is TAction
         select (TAction)action)
           .Via(RestartFlow.OnFailuresWithBackoff(
                () => runner, RestartSettings.Create(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(10), 1)))
           .RunWith(ToDispatch, Store.Materializer);
    }
}