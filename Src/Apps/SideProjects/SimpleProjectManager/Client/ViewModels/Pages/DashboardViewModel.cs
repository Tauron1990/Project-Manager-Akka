using SimpleProjectManager.Client.Data;
using Stl.Fusion;
using Tauron.Application.Blazor;

namespace SimpleProjectManager.Client.ViewModels;

public sealed class DashboardViewModel : BlazorViewModel
{
    public IObservable<long> JobCountState { get; }
    
    public DashboardViewModel(IStateFactory stateFactory, GlobalState globalState)
        : base(stateFactory)
    {
        JobCountState = globalState.Jobs.ActiveJobsCount;
    }
}