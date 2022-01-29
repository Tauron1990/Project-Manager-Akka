
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
    
    public JobsState JobsState { get; }

    public FilesState FilesState { get; }
    
    public ErrorState ErrorState { get; }
    
    public GlobalState(IStateFactory stateFactory)
    {
        var configuration = stateFactory.Services.GetRequiredService<IStoreConfiguration>();
        OnlineMonitor = stateFactory.Services.GetRequiredService<IOnlineMonitor>();
        
        JobsState = new JobsState(configuration, stateFactory);
        ErrorState = new ErrorState(configuration, stateFactory);
        FilesState = new FilesState(stateFactory);
        
        RootStore = configuration.Build();
    }

    public void Dispatch(object action)
        => RootStore.Dispatch(action);

    public void Dispose()
        => RootStore.Dispose();
}