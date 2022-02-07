using System.Collections.Immutable;
using System.Reactive.Linq;
using SimpleProjectManager.Client.ViewModels;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Services;
using Tauron;
using Tauron.Applicarion.Redux.Configuration;
using Tauron.Application.Blazor;

namespace SimpleProjectManager.Client.Data.States;

public partial class JobsState
{
    private IStateConfiguration<InternalJobData> ConfigurateEditor(IStateConfiguration<InternalJobData> configuration)
        => configuration.ApplyRequests(
            f =>
            {
                f.AddRequest(JobDataRequests.PostJobCommit(_service, _eventAggregator), JobDataPatcher.PatchEditorCommit);
            });

    public string ValidateProjectName(string? arg)
    {
        var name = new ProjectName(arg ?? string.Empty);
        var result = _nameValidator.Validate(name);
        
        return result.IsValid ? string.Empty : string.Join(", ", result.Errors.Select(err => err.ErrorMessage));
    }
    
    public IObservable<JobEditorData?> GetJobEditorData(IObservable<string> idObservable)
        => idObservable.SelectMany
        (
            id => TimeoutToken.WithDefault(
                CancellationToken.None,
                async token => string.IsNullOrWhiteSpace(id) ? null : new JobEditorData(await _service.GetJobData(new ProjectId(id), token))).AsTask()
        );

    public async Task CommitJobData(JobEditorCommit newData, Action onCompled)
    {
        if (newData.JobData.OldData == null)
        {
            _eventAggregator.PublishError("Keine Original Daten zur verfügung gestellt");

            return;
        }

        var command = UpdateProjectCommand.Create(newData.JobData.NewData, newData.JobData.OldData);
        var validationResult = await _upodateProjectValidator.ValidateAsync(command);

        if (validationResult.IsValid)
        {
            if (await _eventAggregator.IsSuccess(() => TimeoutToken.WithDefault(default, t => _service.UpdateJobData(command, t)))
             && await _eventAggregator.IsSuccess(async () => await newData.Upload()))
                onCompled();
        }
        else
        {
            var err = string.Join(", ", validationResult.Errors.Select(f => f.ErrorMessage));
            _eventAggregator.PublishWarnig(err);
        }
    }

    public string? TryExtrectName(string name)
    {
        var upperName = name.ToUpper().AsSpan();

        var index = upperName.IndexOf("BM");

        if (index == -1) return null;

        while (upperName.Length >= 10)
        {
            upperName = upperName[index..];
            if(upperName.Length != 10) break;

            if (AllDigit(upperName[2..2]) &&
                upperName[4] == '_' &&
                AllDigit(upperName[5..10]))
            {
                return upperName[..10].ToString();
            }
        }

        return null;
    }

    private bool AllDigit(in ReadOnlySpan<char> input)
    {
        foreach (var t in input)
        {
            if(char.IsDigit(t)) continue;

            return false;
        }

        return true;
    }
    
    public JobEditorCommit CreateNewJobData(JobEditorData editorData, Func<Task<string>> start)
    {
        var data = editorData.OriginalData;
        if (data != null)
        {
            data = data with
                   {
                       JobName = new ProjectName(editorData.JobName ?? string.Empty),
                       Status = editorData.Status,
                       Deadline = ProjectDeadline.FromDateTime(editorData.Deadline),
                       Ordering = GetOrdering(data.Id)
                   };
        }
        else
        {
            var name = new ProjectName(editorData.JobName ?? string.Empty);
            var id = ProjectId.For(name);
            data = new JobData(id,  name, editorData.Status, GetOrdering(id), 
                ProjectDeadline.FromDateTime(editorData.Deadline), ImmutableList<ProjectFileId>.Empty);
        }

        SortOrder? GetOrdering(ProjectId id)
        {
            if (editorData.Ordering != null)
            {
                return data?.Ordering == null 
                    ? new SortOrder(id, editorData.Ordering.Value, false) 
                    : data.Ordering.WithCount(editorData.Ordering.Value);
            }

            return data?.Ordering;
        }

        return new JobEditorCommit(new JobEditorPair<JobData>(data, editorData.OriginalData), start);
    }
}