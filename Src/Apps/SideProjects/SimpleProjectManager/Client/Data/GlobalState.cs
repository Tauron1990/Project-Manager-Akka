using SimpleProjectManager.Client.Data.Core;
using SimpleProjectManager.Client.Data.States;

namespace SimpleProjectManager.Client.Data;

public sealed class GlobalState
{
    public IOnlineMonitor OnlineMonitor { get; }

    public IObservable<bool> IsOnline => OnlineMonitor.Online;

    public IRootStore RootStore { get; }
    
    public JobsState JobsState { get; }

    public ErrorState ErrorState { get; }
    
    public GlobalState(IServiceProvider serviceProvider)
    {
        var configuration = serviceProvider.GetRequiredService<IStoreConfiguration>();
        
        OnlineMonitor = serviceProvider.GetRequiredService<IOnlineMonitor>();
        JobsState = new JobsState(configuration, serviceProvider);
        ErrorState = new ErrorState(configuration, serviceProvider);
        
        RootStore = configuration.Build();
    }
}