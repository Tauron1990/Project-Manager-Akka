using Vogen;

namespace SimpleProjectManager.Shared;

[ValueObject(typeof(string))]
[Instance("NoNewError", "no-new-error")]
[Instance("NewError", "new-Error")]
public readonly partial struct AggregateError
{
    private static Validation Validate(string value)
        => value.ValidateNotNullOrEmpty(nameof(AggregateError));
}