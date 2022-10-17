using Vogen;

namespace Tauron.Application.Master.Commands;

[ValueObject(typeof(string))]
[Instance("Empty", "")]
[Instance("All", "All")]
public readonly partial struct AppName
{
    private static Validation Validate(string value)
        => string.IsNullOrWhiteSpace(value)
            ? Validation.Invalid("App name Should not be Empty")
            : Validation.Ok;
}