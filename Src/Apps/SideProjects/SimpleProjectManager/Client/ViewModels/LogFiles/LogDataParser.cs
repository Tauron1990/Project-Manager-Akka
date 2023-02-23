using System.Collections.Immutable;
using System.Text.Json;

namespace SimpleProjectManager.Client.ViewModels.LogFiles;

public static class LogDataParser
{
    public static IEnumerable<LogData> ParseLogs(string logFile)
    {
        if(string.IsNullOrWhiteSpace(logFile)) yield break;
        
        using var reader = new StringReader(logFile);

        foreach (string line in ReadLines())
        {
            using JsonDocument json = JsonDocument.Parse(line);

            string time = json.RootElement.GetProperty("time").GetString() ?? string.Empty;
            string level = json.RootElement.GetProperty("level").GetString() ?? string.Empty;
            string type = json.RootElement.GetProperty("eventType").GetString() ?? string.Empty;
            string message = json.RootElement.GetProperty("message").GetString() ?? string.Empty;
            
            using JsonDocument propsText = JsonDocument.Parse(json.RootElement.GetProperty("Properties").GetString() ?? string.Empty);
            var propertys = propsText.RootElement.EnumerateObject()
                .ToImmutableDictionary(p => p.Name, p => p.Value.GetString() ?? string.Empty);

            yield return new LogData(time, level, type, message, propertys);
        }
        
        IEnumerable<string> ReadLines()
        {
            while (true)
            {
                // ReSharper disable once AccessToDisposedClosure
                string? line = reader.ReadLine();
                if(string.IsNullOrWhiteSpace(line))
                    break;

                yield return line;
            }
        }
    }
}