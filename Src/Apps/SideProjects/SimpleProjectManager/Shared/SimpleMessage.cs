using Vogen;

namespace SimpleProjectManager.Shared;

[ValueObject(typeof(string))]
[Instance("Empty", "")]
#pragma warning disable MA0097
public readonly partial struct SimpleMessage : IStringValueType<SimpleMessage>
    #pragma warning restore MA0097
{
    static SimpleMessage IStringValueType<SimpleMessage>.GetEmpty => Empty;

    private static Validation Validate(string value)
        => value.ValidateNotNullOrEmpty(nameof(SimpleMessage));
}