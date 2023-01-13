using Vogen;

namespace SimpleProjectManager.Shared;

[ValueObject(typeof(string))]
public readonly partial struct PropertyName : IStringValueType<PropertyName>
{
    private static Validation Validate(string value)
        => value.ValidateNotNullOrEmpty(nameof(PropertyName));

    static PropertyName IStringValueType<PropertyName>.GetEmpty => From("");
}