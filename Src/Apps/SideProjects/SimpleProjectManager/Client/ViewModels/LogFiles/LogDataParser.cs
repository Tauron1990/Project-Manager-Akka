namespace SimpleProjectManager.Client.ViewModels.LogFiles;

public static class LogDataParser
{
    public static IEnumerable<LogData> ParseLogs(string logFile)
    {
        using var reader = new StringReader(logFile);
    }
}