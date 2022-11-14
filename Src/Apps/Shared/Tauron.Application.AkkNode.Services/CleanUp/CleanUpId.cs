using Vogen;

namespace Tauron.Application.AkkaNode.Services.CleanUp;

[ValueObject(typeof(string))]
[Instance("Empty", "")]
#pragma warning disable MA0048
#pragma warning disable MA0097
public readonly partial struct CleanUpId
    #pragma warning restore MA0097
    #pragma warning restore MA0048
{
    private static Validation Validate(string value)
        => string.IsNullOrWhiteSpace(value)
            ? Validation.Invalid("The id is Empty")
            : Validation.Ok;
}