using System;
using ReactiveUI;
using SimpleProjectManager.Client.Shared.Data;

namespace SimpleProjectManager.Client.Shared.ViewModels.Pages;

public sealed class DashboardViewModel : ReactiveObject
{
    public IObservable<long> JobCountState { get; }
    
    public DashboardViewModel(GlobalState globalState)
        => JobCountState = globalState.Jobs.ActiveJobsCount;
}