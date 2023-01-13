using Vogen;

namespace Tauron.Application.Master.Commands.ServiceRegistry;

[ValueObject(typeof(string))]
[Instance("Empty", "")]
#pragma warning disable MA0097
public readonly partial struct ServiceName
    #pragma warning restore MA0097
{
    private static Validation Validate(string value)
        => string.IsNullOrWhiteSpace(value) ? Validation.Invalid("Service name should not be Empty") : Validation.Ok;
}