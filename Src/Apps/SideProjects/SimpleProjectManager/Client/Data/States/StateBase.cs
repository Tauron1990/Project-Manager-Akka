using SimpleProjectManager.Client.Data.Core;

namespace SimpleProjectManager.Client.Data.States;

public abstract class StateBase<TState> where TState : class, new()
{
    protected Action<object> Dispatch { get; private set; } =_ => { };

    protected StateBase(IStoreConfiguration storeConfiguration)
    {
        storeConfiguration.NewState<TState>(
            s => ConfigurateState(s).AndFinish(
                store =>
                {
                    Dispatch = store.Dispatch;
                    PostConfiguration(store.ForState<TState>());
                }));
    }

    protected abstract IStateConfiguration<TState> ConfigurateState(ISourceConfiguration<TState> configuration);

    protected virtual void PostConfiguration(IRootStoreState<TState> state)
    {
        
    }
}