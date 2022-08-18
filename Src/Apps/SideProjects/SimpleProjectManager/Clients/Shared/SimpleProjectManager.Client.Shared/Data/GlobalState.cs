using System;
using Microsoft.Extensions.DependencyInjection;
using SimpleProjectManager.Client.Shared.Data.States;
using SimpleProjectManager.Client.Shared.Data.States.JobState;
using Stl.Fusion;
using Tauron.Applicarion.Redux;
using Tauron.Applicarion.Redux.Configuration;

namespace SimpleProjectManager.Client.Shared.Data;

public sealed class GlobalState : IDisposable
{
    public IStateFactory StateFactory { get; }
    
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
        StateFactory = stateFactory;
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
        {
            var state = ActivatorUtilities.CreateInstance<TState>(serviceProvider, stateFactory);

            if(state is IStoreInitializer baseState)
                baseState.RunConfig(configuration);

            return state;
        }
    }

    public void Dispatch(object action)
        => RootStore.Dispatch(action);

    public void Dispose()
    {
        _scope.Dispose();
        RootStore.Dispose();
    }
}