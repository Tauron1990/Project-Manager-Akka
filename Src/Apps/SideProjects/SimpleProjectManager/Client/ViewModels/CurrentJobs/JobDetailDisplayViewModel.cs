using System.Reactive;
using Microsoft.AspNetCore.Components;
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

    public ReactiveCommand<ProjectId?, Unit> EditJobs { get; }
    
    public JobDetailDisplayViewModel(
        IStateFactory stateFactory, JobsViewModel jobsModel, IJobDatabaseService jobDatabaseService,
        NavigationManager navigationManager, IEventAggregator aggregator) : base(stateFactory)
    {
        _jobsModel = jobsModel;
        _jobDatabaseService = jobDatabaseService;
        EditJobs = ReactiveCommand.Create<ProjectId?, Unit>(
            id =>
            {
                if (id is null)
                {
                    aggregator.PublishWarnig("Keine Prijekt id Verfügabr");

                    return Unit.Default;
                }

                navigationManager.NavigateTo($"/EditJob/{id.Value}");

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