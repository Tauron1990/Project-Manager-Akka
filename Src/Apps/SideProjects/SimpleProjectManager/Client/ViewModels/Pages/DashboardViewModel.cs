using SimpleProjectManager.Shared.Services;
using Stl.Fusion;
using Tauron.Application.Blazor;

namespace SimpleProjectManager.Client.ViewModels;

public sealed class DashboardViewModel : BlazorViewModel
{
    public IState<long> JobCountState { get; }
    
    public DashboardViewModel(IStateFactory stateFactory, IJobDatabaseService databaseService)
        : base(stateFactory)
    {
        JobCountState = stateFactory.NewComputed(
            new ComputedState<long>.Options(),
            async (_, t) => await databaseService.CountActiveJobs(t));
    }
}