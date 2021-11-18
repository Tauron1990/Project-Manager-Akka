using System.Reactive;
using System.Reactive.Linq;
using ReactiveUI;
using SimpleProjectManager.Shared.Services;
using Stl.Fusion;
using Tauron;
using Tauron.Application.Blazor;

namespace SimpleProjectManager.Client.ViewModels;

public sealed class CurrentJobsViewModel : StatefulViewModel<JobInfo[]>
{
    private readonly IJobDatabaseService _databaseService;

    public ReactiveCommand<Unit, Unit> NewJob { get; }
    
    public CurrentJobsViewModel(IStateFactory stateFactory, IJobDatabaseService databaseService, JobsViewModel jobsViewModel, PageNavigation pageNavigation)
        : base(stateFactory)
    {
        _databaseService = databaseService;
        
        NewJob = ReactiveCommand.Create(pageNavigation.NewJob);
        
        (from newJobs in NextElement
         where newJobs is not null && jobsViewModel.Current is not null
                                   && !newJobs.Contains(jobsViewModel.Current.Info)
         select default(JobSortOrderPair))
           .Subscribe(jobsViewModel.Publish)
           .DisposeWith(this);
    }

    protected override async Task<JobInfo[]> ComputeState(CancellationToken cancellationToken)
        => await _databaseService.GetActiveJobs(cancellationToken);
}