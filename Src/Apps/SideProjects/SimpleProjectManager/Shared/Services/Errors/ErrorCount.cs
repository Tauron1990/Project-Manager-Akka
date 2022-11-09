using Vogen;

namespace SimpleProjectManager.Shared.Services;

[ValueObject(typeof(long))]
public readonly partial struct ErrorCount
{
    private static Validation Validate(long value)
        => value < 0 ? Validation.Invalid("Error Count must be Greater then zero") : Validation.Ok;
}