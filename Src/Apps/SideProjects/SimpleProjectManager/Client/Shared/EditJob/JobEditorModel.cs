using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Services;

namespace SimpleProjectManager.Client.Shared.EditJob;

public class JobEditorModel
{
    public string? JobName { get; set; }

    public ProjectStatus Status { get; set; }
    
    public long Ordering { get; set; } = long.MaxValue;
    
    public DateTimeOffset? Deadline { get; set; }

    public JobEditorModel(JobData? data)
    {
        if(data == null) return;

        JobName = data.JobName.Value;
        Status = data.Status;
        Ordering = data.Ordering.Value;
        Deadline = data.Deadline?.Value;
    }

    public Func<string, string?> ValidateJobName = ValidateJobNameImpl;

    private static string? ValidateJobNameImpl(string arg)
    {
        if (string.IsNullOrWhiteSpace(arg))
            return "Es muss ein name angegeben werden";

        if (arg.ToUpper().StartsWith("BM"))
        {

        }

        return null;
    }
}