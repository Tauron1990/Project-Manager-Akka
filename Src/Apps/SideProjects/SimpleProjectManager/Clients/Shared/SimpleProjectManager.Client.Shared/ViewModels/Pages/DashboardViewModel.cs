using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using SimpleProjectManager.Client.Shared.Data;
using SimpleProjectManager.Shared.Services;

namespace SimpleProjectManager.Client.Shared.ViewModels.Pages;

public sealed class DashboardViewModel : ReactiveObject
{
    public DashboardViewModel(GlobalState globalState)
    {
        JobCountState = globalState.Jobs.ActiveJobsCount;
        StartJob = ReactiveCommand.CreateFromTask(StartJobExec, JobCountState.Select(aj => aj.Value > 0));
    }

    private async Task StartJobExec()
    {
        
    }

    public IObservable<ActiveJobs> JobCountState { get; }

    public ReactiveCommand<Unit, Unit> StartJob { get; }
}