using System.Reactive;
using ReactiveUI;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Services;
using Stl.Fusion;
using Tauron.Application;
using Tauron.Application.Blazor;

namespace SimpleProjectManager.Client.ViewModels;

public class JobDetailDisplayViewModel : StatefulViewModel<JobData?>
{
    private readonly JobsViewModel _jobsModel;
    private readonly IJobDatabaseService _jobDatabaseService;

    public ReactiveCommand<ProjectId?, Unit> EditJob { get; }
    
    public JobDetailDisplayViewModel(
        IStateFactory stateFactory, JobsViewModel jobsModel, IJobDatabaseService jobDatabaseService,
        PageNavigation navigationManager, IEventAggregator aggregator) : base(stateFactory)
    {
        _jobsModel = jobsModel;
        _jobDatabaseService = jobDatabaseService;
        EditJob = ReactiveCommand.Create<ProjectId?, Unit>(
            id =>
            {
                if (id is null)
                {
                    aggregator.PublishWarnig("Keine Projekt id Verfügbar");

                    return Unit.Default;
                }

                navigationManager.EditJob(id);

                return Unit.Default;
            });
    }

    protected override async Task<JobData?> ComputeState(CancellationToken cancellationToken)
    {
        var currentSelected = await _jobsModel.CurrentJobState.Use(cancellationToken);
        if (currentSelected == null) return null;
        
        return await _jobDatabaseService.GetJobData(currentSelected.Info.Project, cancellationToken);
    }
}