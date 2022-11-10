using Vogen;

namespace SimpleProjectManager.Client.Shared.Data.Files;

[ValueObject(typeof(long))]
[Instance("Max", long.MaxValue)]
public readonly partial struct MaxSize
{
    private static Validation Validate(long value)
        => value < 0 ? Validation.Invalid("Size must be Positive") : Validation.Ok;
}