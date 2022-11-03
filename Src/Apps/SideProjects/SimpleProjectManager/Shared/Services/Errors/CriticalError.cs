using System.Collections.Immutable;

namespace SimpleProjectManager.Shared.Services;

public sealed record ErrorProperty(string Key, string Value);

public sealed record CriticalError(ErrorId Id, DateTime Occurrence, string ApplicationPart, string Message, StackTraceData? StackTrace, ImmutableList<ErrorProperty> ContextData)
{
    public static readonly CriticalError Empty = new(ErrorId.New, DateTime.MinValue, "", "", null, ImmutableList<ErrorProperty>.Empty);
}