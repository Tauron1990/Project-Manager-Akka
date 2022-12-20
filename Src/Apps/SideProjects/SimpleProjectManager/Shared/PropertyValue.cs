using FluentValidation.Validators;
using Vogen;

namespace SimpleProjectManager.Shared;

[ValueObject(typeof(string))]
[Instance("Empty", "")]
#pragma warning disable MA0097
public readonly partial struct PropertyValue : IStringValueType<PropertyValue>
    #pragma warning restore MA0097
{
    private static Validation Validate(string value)
        => value.ValidateNotNullOrEmpty(nameof(PropertyValue));

    static PropertyValue IStringValueType<PropertyValue>.GetEmpty => Empty;
}