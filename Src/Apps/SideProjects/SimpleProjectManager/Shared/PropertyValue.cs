using Vogen;

namespace SimpleProjectManager.Shared;

[ValueObject(typeof(string))]
[Instance("Empty", "")]
public readonly partial struct PropertyValue
{
    private static Validation Validate(string value)
        => value.ValidateNotNullOrEmpty(nameof(PropertyValue));
}