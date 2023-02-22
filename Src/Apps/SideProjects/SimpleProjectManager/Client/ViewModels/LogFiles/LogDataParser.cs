using System.Text.Json;
using Akkatecture.Extensions;

namespace SimpleProjectManager.Client.ViewModels.LogFiles;

public static class LogDataParser
{
    public static IEnumerable<LogData> ParseLogs(string logFile)
    {
        using var reader = new StringReader(logFile);

        foreach (string line in ReadLines())
        {
            using JsonDocument json = JsonDocument.Parse(line);

            string time = json.RootElement.GetProperty("time").GetString() ?? string.Empty;
            string level = json.RootElement.GetProperty("level").GetString() ?? string.Empty;
            string type = json.RootElement.GetProperty("eventType").GetString() ?? string.Empty;
            string message = json.RootElement.GetProperty("message").GetString() ?? string.Empty;
            string props = json.RootElement.GetProperty("Properties").GetString() ?? string.Empty;
            
        }
        
        IEnumerable<string> ReadLines()
        {
            while (true)
            {
                string? line = reader.ReadLine();
                if(string.IsNullOrWhiteSpace(line))
                    break;

                yield return line;
            }
        }
    }
}