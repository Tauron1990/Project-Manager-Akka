using System;
using ReactiveUI;
using SimpleProjectManager.Client.Shared.Data;
using SimpleProjectManager.Shared.Services;

namespace SimpleProjectManager.Client.Shared.ViewModels.Pages;

public sealed class DashboardViewModel : ReactiveObject
{
    public DashboardViewModel(GlobalState globalState)
        => JobCountState = globalState.Jobs.ActiveJobsCount;

    public IObservable<ActiveJobs> JobCountState { get; }
}