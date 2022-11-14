using Vogen;

namespace SimpleProjectManager.Shared;

[ValueObject(typeof(string))]
[Instance("Empty", "")]
#pragma warning disable MA0097
public readonly partial struct SimpleMessage
    #pragma warning restore MA0097
{
    private static Validation Validate(string value)
        => value.ValidateNotNullOrEmpty(nameof(SimpleMessage));
}