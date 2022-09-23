using System;
using System.Collections.Generic;
using System.Linq;
using ReactiveUI;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Services;

namespace SimpleProjectManager.Client.Shared.Data.JobEdit;

public class JobEditorData : ReactiveObject
{
    private readonly bool _isUpdate;
    private string? _jobName;
    
    public JobData? OriginalData { get; }

    public string? JobName
    {
        get => _jobName;
        set => this.RaiseAndSetIfChanged(ref _jobName, value);
    }

    public ProjectStatus Status { get; set; }
    
    public int? Ordering { get; set; }
    
    public DateTime? Deadline { get; set; }

    public JobEditorData(JobData? originalData)
    {
        OriginalData = originalData;
        _isUpdate = originalData != null;

        if(originalData == null) return;

        JobName = originalData.JobName.Value;
        Status = originalData.Status;
        Ordering = originalData.Ordering?.SkipCount;
        Deadline = originalData.Deadline?.Value.ToLocalTime().Date;
    }
}