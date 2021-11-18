using Microsoft.AspNetCore.Components;
using SimpleProjectManager.Client.ViewModels;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Validators;
using Tauron.Application.Blazor;

namespace SimpleProjectManager.Client.Pages;

public partial class EditJob
{
    [Parameter]
    public string ProjectId { get; set; } = string.Empty;

    protected override async Task<JobEditorData?> ComputeState(CancellationToken cancellationToken) 
        => string.IsNullOrWhiteSpace(ProjectId) 
            ? null : new JobEditorData(await _jobDatabase.GetJobData(new ProjectId(ProjectId), cancellationToken));

    private void Cancel()
        => _navigationManager.NavigateTo("/");

    private async Task Commit(JobEditorCommit newData)
    {
        if (newData.JobData.OldData == null)
        {
            _eventAggregator.PublishError("Keine Original Daten zur verfügung gestellt");
            return;
        }

        var command = UpdateProjectCommand.Create(newData.JobData.NewData, newData.JobData.OldData);
        var validator = new UpdateProjectCommandValidator();
        var validationResult = await validator.ValidateAsync(command);

        if (validationResult.IsValid)
        {
            if (await _eventAggregator.IsSuccess(() => TimeoutToken.WithDefault(t => _jobDatabase.UpdateJobData(command, t))))
                _navigationManager.NavigateTo("/");
        }
        else
        {
            var err = string.Join(", ", validationResult.Errors.Select(f => f.ErrorMessage));
            _eventAggregator.PublishWarnig(err);
        }
    }
}