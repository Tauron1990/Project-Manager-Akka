using System;
using System.Reactive;
using ReactiveUI;
using SimpleProjectManager.Client.Shared.Data;
using SimpleProjectManager.Client.Shared.Data.States;

namespace SimpleProjectManager.Client.Shared.ViewModels.Pages;

public sealed class CurrentJobsViewModel : ReactiveObject
{
    public IObservable<JobSortOrderPair[]> Jobs { get; }
    
    public ReactiveCommand<Unit, Unit> NewJob { get; }
    
    public CurrentJobsViewModel(GlobalState state, PageNavigation pageNavigation)
    {
        Jobs = state.Jobs.CurrentJobs;
        NewJob = ReactiveCommand.Create(pageNavigation.NewJob, state.IsOnline);
    }
}