using System.Reactive;
using ReactiveUI;
using SimpleProjectManager.Client.Pages;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Services;
using SimpleProjectManager.Shared.Validators;
using Stl.Fusion;
using Tauron.Application;
using Tauron.Application.Blazor;

namespace SimpleProjectManager.Client.ViewModels;

public sealed class EditJobViewModel : StatefulViewModel<JobEditorData?>
{
    private readonly UpdateProjectCommandValidator _validator = new();
    private readonly IJobDatabaseService _databaseService;
    private IState<string> JobId { get; }

    public Action Cancel { get; }
    
    public Action<JobEditorCommit> Commit { get; }

    public EditJobViewModel(IStateFactory stateFactory, IJobDatabaseService databaseService, PageNavigation pageNavigation, IEventAggregator eventAggregator)
        : base(stateFactory)
    {
        _databaseService = databaseService;
        
        JobId = GetParameter<string>(nameof(EditJob.ProjectId));
        Cancel = pageNavigation.ShowStartPage;

        var commit = ReactiveCommand.CreateFromTask<JobEditorCommit, Unit>(
            async newData =>
            {
                if (newData.JobData.OldData == null)
                {
                    eventAggregator.PublishError("Keine Original Daten zur verfügung gestellt");
                    return Unit.Default;
                }

                var command = UpdateProjectCommand.Create(newData.JobData.NewData, newData.JobData.OldData);
                var validationResult = await _validator.ValidateAsync(command);

                if (validationResult.IsValid)
                {
                    if (await eventAggregator.IsSuccess(() => TimeoutToken.WithDefault(t => databaseService.UpdateJobData(command, t))))
                        pageNavigation.ShowStartPage();
                }
                else
                {
                    var err = string.Join(", ", validationResult.Errors.Select(f => f.ErrorMessage));
                    eventAggregator.PublishWarnig(err);
                }

                return Unit.Default;
            });
        Commit = commit.ToAction();
    }


    protected override async Task<JobEditorData?> ComputeState(CancellationToken cancellationToken)
    {
        var id = await JobId.Use(cancellationToken);

        return string.IsNullOrWhiteSpace(id) 
            ? null : new JobEditorData(await _databaseService.GetJobData(new ProjectId(id), cancellationToken));
    }
}