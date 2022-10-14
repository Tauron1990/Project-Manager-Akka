using Vogen;

namespace Tauron.Application.AkkaNode.Services.CleanUp;

[ValueObject(typeof(string))]
[Instance("Empty", "")]
public readonly partial struct CleanUpId
{
    private static Validation Validate(string value)
        => string.IsNullOrWhiteSpace(value)
            ? Validation.Invalid("The id is Empty")
            : Validation.Ok;
}