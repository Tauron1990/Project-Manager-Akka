using Vogen;

namespace SimpleProjectManager.Shared;

[ValueObject(typeof(string))]
public readonly partial struct PropertyName
{
    private static Validation Validate(string value)
        => value.ValidateNotNullOrEmpty(nameof(PropertyName));
}