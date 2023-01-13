using Vogen;

namespace TestApp.Test2;

[ValueObject(typeof(string))]
[Instance("Empty", "")]
public readonly partial struct ToSerialize
{
    private static Validation Validate(string value)
        => string.IsNullOrWhiteSpace(value) ? Validation.Invalid("Should not Null") : Validation.Ok;
}