namespace SimpleProjectManager.Client.Data.Core;

public interface IStoreConfiguration
{
    IStoreConfiguration NewState<TState>(Func<ISourceConfiguration<TState>, IConfiguredState> configurator)
        where TState : class, new();

    IRootStore Build();
}