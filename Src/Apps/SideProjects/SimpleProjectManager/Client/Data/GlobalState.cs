
using SimpleProjectManager.Client.Data.States;
using Stl.Fusion;
using Tauron.Applicarion.Redux;
using Tauron.Applicarion.Redux.Configuration;

namespace SimpleProjectManager.Client.Data;

public sealed class GlobalState : IDisposable
{
    private readonly IDisposable _scope;

    public IOnlineMonitor OnlineMonitor { get; }

    public IObservable<bool> IsOnline => OnlineMonitor.Online;

    public IRootStore RootStore { get; }
    
    public JobsState Jobs { get; }

    public FilesState Files { get; }
    
    public ErrorState Errors { get; }

    public TaskState Tasks { get; }
    
    public GlobalState(IStateFactory stateFactory, IServiceProvider rootProvider)
    {
        var scope = rootProvider.CreateScope();
        _scope = scope;
        var serviceProvider = scope.ServiceProvider;

        var configuration = serviceProvider.GetRequiredService<IStoreConfiguration>();
        OnlineMonitor = serviceProvider.GetRequiredService<IOnlineMonitor>();
        
        Jobs = CreateState<JobsState>();
        Errors = CreateState<ErrorState>();
        Files = CreateState<FilesState>();
        Tasks = CreateState<TaskState>();
        
        RootStore = configuration.Build();
        
        TState CreateState<TState>()
            => ActivatorUtilities.CreateInstance<TState>(serviceProvider, configuration, stateFactory);
    }

    public void Dispatch(object action)
        => RootStore.Dispatch(action);

    public void Dispose()
    {
        _scope.Dispose();
        RootStore.Dispose();
    }
}