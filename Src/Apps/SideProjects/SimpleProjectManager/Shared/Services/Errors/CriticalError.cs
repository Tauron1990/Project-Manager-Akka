using System.Collections.Immutable;

namespace SimpleProjectManager.Shared.Services;

public sealed record CriticalError(string Id, DateTime Occurrence, string ApplicationPart, string Message, string? StackTrace, ImmutableDictionary<string, string> ContextData);