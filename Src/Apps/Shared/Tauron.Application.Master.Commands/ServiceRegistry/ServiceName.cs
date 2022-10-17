using Vogen;

namespace Tauron.Application.Master.Commands.ServiceRegistry;

[ValueObject(typeof(string))]
[Instance("Empty", "")]
public readonly partial struct ServiceName
{
    private static Validation Validate(string value)
        => string.IsNullOrWhiteSpace(value) ? Validation.Invalid("Service name should not be Empty") : Validation.Ok;
}