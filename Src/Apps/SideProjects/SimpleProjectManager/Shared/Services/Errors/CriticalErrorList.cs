using System.Collections.Immutable;

namespace SimpleProjectManager.Shared.Services;

public readonly struct CriticalErrorList
{
    public ImmutableArray<CriticalError> Errors { get; }

    public CriticalErrorList(ImmutableArray<CriticalError> errors)
        => Errors = errors;
}