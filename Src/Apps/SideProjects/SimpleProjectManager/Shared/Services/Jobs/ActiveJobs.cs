using Vogen;

namespace SimpleProjectManager.Shared.Services;

[ValueObject(typeof(long))]
public readonly partial struct ActiveJobs
{
    private static Validation Validate(long value)
        => value >= 0 ? Validation.Ok : Validation.Invalid("Cound Should be Posotive");
}