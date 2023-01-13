using Vogen;

namespace Tauron.Application.Master.Commands;

[ValueObject(typeof(string))]
[Instance("Empty", "")]
[Instance("All", "All")]
#pragma warning disable MA0097
public readonly partial struct AppName
    #pragma warning restore MA0097
{
    private static Validation Validate(string value)
        => string.IsNullOrWhiteSpace(value)
            ? Validation.Invalid("App name Should not be Empty")
            : Validation.Ok;
}