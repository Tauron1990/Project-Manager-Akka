using System.Collections.Immutable;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Microsoft.AspNetCore.Components;
using ReactiveUI;
using SimpleProjectManager.Client.Shared.EditJob;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Services;
using Stl.Fusion;
using Tauron;
using Tauron.Application;
using Tauron.Application.Blazor;

namespace SimpleProjectManager.Client.ViewModels;

public sealed class JobEditorViewModel : BlazorViewModel
{
    private readonly BehaviorSubject<bool> _isValid;

    public bool IsValid => _isValid.Value;
    public Action<bool> IsValidChanged => _isValid.OnNext;

    private readonly ObservableAsPropertyHelper<JobEditorData?> _data;

    public JobEditorData? Data => _data.Value;

    public ReactiveCommand<Unit, Unit> Cancel { get; }
    
    public ReactiveCommand<Unit, Unit> Commit { get; }

    public FileUploadTrigger FileUploadTrigger { get; }

    public FileUploaderViewModel UploaderViewModel { get; }

    public JobEditorViewModel(IStateFactory stateFactory, IEventAggregator aggregator, FileUploaderViewModel uploaderViewModel)
        : base(stateFactory)
    {
        UploaderViewModel = uploaderViewModel;

        _isValid = new BehaviorSubject<bool>(false).DisposeWith(this);
        FileUploadTrigger = new FileUploadTrigger();

        var commitEvent = GetParameter<EventCallback<JobEditorCommit>>(nameof(JobEditor.Commit));
        var cancelEvent = GetParameter<EventCallback>(nameof(JobEditor.Cancel));
        var canCancel = GetParameter<bool>(nameof(JobEditor.CanCancel));

        _data = GetParameter<JobEditorData?>(nameof(JobEditor.Data))
           .ToObservable()
           .Select(i => i ?? new JobEditorData(null))
           .ToProperty(this, m => m.Data)
           .DisposeWith(this);

        Cancel = ReactiveCommand.CreateFromTask(
            async () =>
            {
                if (!cancelEvent.HasValue) return;

                await cancelEvent.Value.InvokeAsync();
            }, canCancel.ToObservable().StartWith(false))
           .DisposeWith(this);

        Commit = ReactiveCommand.CreateFromTask(CreateCommit, _isValid);

        async Task CreateCommit()
        {
            if(!commitEvent.HasValue) return;

            if (Data == null)
            {
                aggregator.PublishError("Keine Daten Verfügbar");
                return;
            }

            await commitEvent.Value.InvokeAsync(CreateNewJobData(Data));
        }
    }

    private JobEditorCommit CreateNewJobData(JobEditorData editorData)
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

        return new JobEditorCommit(new JobEditorPair<JobData>(data, editorData.OriginalData), FileUploadTrigger.Upload);
    }
}