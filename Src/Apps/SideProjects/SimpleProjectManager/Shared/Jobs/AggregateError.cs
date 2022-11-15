using Vogen;

namespace SimpleProjectManager.Shared;

[ValueObject(typeof(string))]
[Instance("NoNewError", "no-new-error")]
[Instance("NewError", "new-Error")]
#pragma warning disable MA0097
public readonly partial struct AggregateError
    #pragma warning restore MA0097
{
    private static Validation Validate(string value)
        => value.ValidateNotNullOrEmpty(nameof(AggregateError));
}