using Vogen;

namespace Tauron.Application.Master.Commands.Administration.Host;

[ValueObject(typeof(string))]
[Instance("Empty", "")]
public readonly partial struct HostName
{
    private static Validation Validate(string value)
        => string.IsNullOrWhiteSpace(value)
            ? Validation.Invalid("Host Name Should not be Empty")
            : Validation.Ok;
}