using System.Collections.Immutable;

namespace SimpleProjectManager.Shared.Services;

public readonly struct CriticalErrorList
{
    public static readonly CriticalErrorList Empty = new(ImmutableList<CriticalError>.Empty);

    public ImmutableList<CriticalError> Errors { get; }

    public CriticalErrorList(ImmutableList<CriticalError> errors)
        => Errors = errors;
}