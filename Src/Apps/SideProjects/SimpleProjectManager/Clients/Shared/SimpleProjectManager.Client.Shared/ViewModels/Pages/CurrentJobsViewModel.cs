using System;
using System.Reactive;
using System.Reactive.Linq;
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
        Console.WriteLine($"{nameof(CurrentJobsViewModel)} Constructor");
        Jobs = state.Jobs.CurrentJobs.Do(l => Console.WriteLine($"{nameof(CurrentJobsViewModel)} -- Recived new Data: {l.Length}"));
        NewJob = ReactiveCommand.Create(pageNavigation.NewJob, state.IsOnline);
    }
}