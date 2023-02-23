using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace SimpleProjectManager.Client.ViewModels.LogFiles;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
public record LogData(string Date, string Level, string EventType, string Message, ImmutableDictionary<string, string> Propertys);