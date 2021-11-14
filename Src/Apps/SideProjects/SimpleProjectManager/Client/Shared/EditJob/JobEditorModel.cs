using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Services;

namespace SimpleProjectManager.Client.Shared.EditJob;

public class JobEditorModel
{
    private readonly bool _isUpdate;

    public JobData? OriginalData { get; }

    public string? JobName { get; set; }

    public ProjectStatus Status { get; set; }
    
    public int? Ordering { get; set; }
    
    public DateTime? Deadline { get; set; }

    public JobEditorModel(JobData? originalData)
    {
        OriginalData = originalData;
        _isUpdate = originalData != null;
        ValidateDeadline = ValidateDeadlineImpl;

        if(originalData == null) return;

        JobName = originalData.JobName.Value;
        Status = originalData.Status;
        Ordering = originalData.Ordering?.SkipCount;
        Deadline = originalData.Deadline?.Value.ToLocalTime().Date;
    }

    public readonly Func<DateTime?, string?> ValidateDeadline;

    private string? ValidateDeadlineImpl(DateTime? arg)
    {
        if(_isUpdate || arg == null) return null;

        return arg > DateTime.Now ? null : "Der Termin muss in der Zukunft liegen";
    }

    public readonly Func<string, string?> ValidateJobName = ValidateJobNameImpl;

    private static string? ValidateJobNameImpl(string arg)
    {
        if (string.IsNullOrWhiteSpace(arg))
            return "Es muss ein name angegeben werden";

        if (!arg.ToUpper().StartsWith("BM")) return null;

        var isOk =
            arg.Length == 10
         && arg[2..2].All(char.IsDigit)
         && arg[4] == '_'
         && arg[5..].All(char.IsDigit);

        return !isOk ? "DIe Auftrags Nummer ist im Falschen Format" : null;
    }
}