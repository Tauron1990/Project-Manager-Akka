using SimpleProjectManager.Client.Data.Core;
using SimpleProjectManager.Client.Data.States;
using SimpleProjectManager.Shared.Services;

namespace SimpleProjectManager.Client.Data;

public sealed class GlobalState
{
    public IRootStore RootStore { get; }
    
    public JobsState JobsState { get; }

    public GlobalState(IStoreConfiguration configuration, IJobDatabaseService jobDatabaseService)
    {
        JobsState = new JobsState(configuration, jobDatabaseService);

        RootStore = configuration.Build();
    }
}