using System.Collections.Immutable;

namespace SimpleProjectManager.Client.ViewModels.LogFiles;

public record LogData(string Date, string Level, string EventType, string Message, ImmutableDictionary<string, string> Propertys);