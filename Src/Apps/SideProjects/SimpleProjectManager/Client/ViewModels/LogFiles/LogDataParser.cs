using System.Collections.Immutable;
using System.Text.Json;

namespace SimpleProjectManager.Client.ViewModels.LogFiles;

public static class LogDataParser
{
    public static IEnumerable<LogData> ParseLogs(string logFileData)
    {
        if(string.IsNullOrWhiteSpace(logFileData)) yield break;
            
        using var mem = new MemoryStream(Convert.FromBase64String(logFileData));
        using var reader = new StreamReader(mem);

        while (true)
        {
            string? line = reader.ReadLine();
            if(string.IsNullOrWhiteSpace(line)) yield break;
            yield return ParseLogLine(line);
        }
    }

    private static LogData ParseLogLine(string line)
    {
        try
        {
            using JsonDocument json = JsonDocument.Parse(line);

            string time = json.RootElement.GetProperty("time").GetString() ?? string.Empty;
            string level = json.RootElement.GetProperty("level").GetString() ?? string.Empty;
            string type = json.RootElement.GetProperty("eventType").GetString() ?? string.Empty;
            string message = json.RootElement.GetProperty("message").GetString() ?? string.Empty;

            using JsonDocument propsText = JsonDocument.Parse(json.RootElement.GetProperty("Properties").GetString() ?? string.Empty);
            var propertys = propsText.RootElement.EnumerateObject()
                .ToImmutableDictionary(p => p.Name, p => p.Value.ToString());

            return new LogData(time, level, type, message, propertys);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error Parse Line: {e}");
            
            return new LogData(
                DateTime.Now.ToString("u"),
                "Critical",
                "-",
#pragma warning disable EPC12
                e.Message,
#pragma warning restore EPC12
                ImmutableDictionary<string, string>.Empty.Add("Fehler", e.ToString()));
        }
    }
}