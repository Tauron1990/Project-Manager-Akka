using System.Collections.Immutable;
using Microsoft.AspNetCore.Components;
using SimpleProjectManager.Client.ViewModels;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Services;

namespace SimpleProjectManager.Client.Shared.EditJob;

public partial class JobEditor
{
    [Parameter]
    public string Title { get; set; } = string.Empty;

    [Parameter]
    public EventCallback<JobEditorCommit> Commit { get; set; }

    [Parameter]
    public EventCallback Cancel { get; set; }

    [Parameter]
    public bool CanCancel { get; set; }
    
    [Parameter]
    public JobEditorConfiguration Configuration { get; set; } = JobEditorConfiguration.Default;

    private bool StatusEditing => Configuration.StatusEditing;

    private bool SortOrderEditing => Configuration.SortOrderEditing;

    [Parameter]
    public JobEditorData? Model { get; set; }

    private bool _isValid;

    private JobEditorData? _model;

    protected override void OnParametersSet()
    {
        _model = Model ?? new JobEditorData(null);
        base.OnParametersSet();
    }

    private async Task CommitChanges()
    {
        if(_model == null) return;

        var data = _model.OriginalData;
        if (data != null)
        {
            data = data with
            {
                JobName = new ProjectName(_model.JobName ?? string.Empty),
                Status = _model.Status,
                Deadline = ProjectDeadline.FromDateTime(_model.Deadline),
                Ordering = GetOrdering(data.Id)
            };
        }
        else
        {
            var name = new ProjectName(_model.JobName ?? string.Empty);
            var id = ProjectId.For(name);
            data = new JobData(id,  name, _model.Status, GetOrdering(id), 
                ProjectDeadline.FromDateTime(_model.Deadline), ImmutableList<ProjectFileId>.Empty);
        }

        await Commit.InvokeAsync(new JobEditorCommit(new JobEditorPair<JobData>(data, _model.OriginalData)));

        SortOrder? GetOrdering(ProjectId id)
        {
            if (_model.Ordering != null)
            {
                return data?.Ordering == null 
                    ? new SortOrder(id, _model.Ordering.Value, false) 
                    : data.Ordering.WithCount(_model.Ordering.Value);
            }

            return data?.Ordering;
        }
    }
}