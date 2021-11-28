using SimpleProjectManager.Shared;

namespace SimpleProjectManager.Client.Shared;

public static class JobTranslator
{
    public static string GetString(ProjectStatus status)
    {
        return status switch
        {
            ProjectStatus.Entered => "Eingegangen",
            ProjectStatus.Pending => "Anstehend",
            ProjectStatus.Running => "Läuft",
            ProjectStatus.Suspended => "Pausiert",
            ProjectStatus.Finished => "Fertig",
            ProjectStatus.ReRun => "Wiederholung",
            ProjectStatus.Deleted => "Gelöscht",
            _ => ""
        };
    }
}