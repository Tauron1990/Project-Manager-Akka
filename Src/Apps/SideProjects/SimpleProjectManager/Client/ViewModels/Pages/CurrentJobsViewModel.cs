using System.Reactive;
using System.Reactive.Linq;
using ReactiveUI;
using SimpleProjectManager.Client.Data;
using SimpleProjectManager.Client.Data.States;
using SimpleProjectManager.Shared.Services;
using Stl.Fusion;
using Tauron;
using Tauron.Application.Blazor;

namespace SimpleProjectManager.Client.ViewModels;

public sealed class CurrentJobsViewModel : BlazorViewModel
{
    public IObservable<JobSortOrderPair[]> Jobs { get; }
    
    public ReactiveCommand<Unit, Unit> NewJob { get; }
    
    public CurrentJobsViewModel(IStateFactory stateFactory, GlobalState state, PageNavigation pageNavigation)
        : base(stateFactory)
    {
        Jobs = state.JobsState.CurrentJobs;
        NewJob = ReactiveCommand.Create(pageNavigation.NewJob);
    }
}