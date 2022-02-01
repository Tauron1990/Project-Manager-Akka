
using SimpleProjectManager.Client.Data.States;
using Stl.Fusion;
using Tauron.Applicarion.Redux;
using Tauron.Applicarion.Redux.Configuration;

namespace SimpleProjectManager.Client.Data;

public sealed class GlobalState : IDisposable
{
    public IOnlineMonitor OnlineMonitor { get; }

    public IObservable<bool> IsOnline => OnlineMonitor.Online;

    public IRootStore RootStore { get; }
    
    public JobsState Jobs { get; }

    public FilesState Files { get; }
    
    public ErrorState Errors { get; }

    public TaskState Tasks { get; }
    
    public GlobalState(IStateFactory stateFactory, IServiceProvider serviceProvider)
    {
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
        => RootStore.Dispose();
}