using System;
using Akkatecture.Jobs;
using ReactiveUI;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Services;

namespace SimpleProjectManager.Client.Shared.Data.JobEdit;

public class JobEditorData : ReactiveObject
{
    private ProjectName? _jobName;

    public JobEditorData(JobData? originalData)
    {
        OriginalData = originalData;

        if(originalData == null) return;

        JobName = originalData.JobName;
        Status = originalData.Status;
        Ordering = originalData.Ordering?.SkipCount;
        Deadline = originalData.Deadline?.Value.ToLocalTime().Date;
    }

    public JobData? OriginalData { get; }

    public ProjectName? JobName
    {
        get => _jobName;
        set => this.RaiseAndSetIfChanged(ref _jobName, value);
    }

    public ProjectStatus Status { get; set; }

    public int? Ordering { get; set; }

    public DateTime? Deadline { get; set; }
}