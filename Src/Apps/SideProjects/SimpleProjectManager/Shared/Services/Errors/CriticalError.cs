using System.Collections.Immutable;

namespace SimpleProjectManager.Shared.Services;

public sealed record ErrorProperty(string Key, string Value);

public sealed record CriticalError(string Id, DateTime Occurrence, string ApplicationPart, string Message, string? StackTrace, ImmutableList<ErrorProperty> ContextData);