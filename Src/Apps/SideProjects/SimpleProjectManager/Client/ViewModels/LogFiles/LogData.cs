using System.Collections.Immutable;

namespace SimpleProjectManager.Client.ViewModels.LogFiles;

public record LogData(string Date, string EventType, string Message, string Type, ImmutableDictionary<string, string> Propertys);