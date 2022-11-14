using Vogen;

namespace Tauron.Application.Master.Commands.Administration.Host;

[ValueObject(typeof(string))]
[Instance("Empty", "")]
#pragma warning disable MA0097
public readonly partial struct HostName
    #pragma warning restore MA0097
{
    private static Validation Validate(string value)
        => string.IsNullOrWhiteSpace(value)
            ? Validation.Invalid("Host Name Should not be Empty")
            : Validation.Ok;
}