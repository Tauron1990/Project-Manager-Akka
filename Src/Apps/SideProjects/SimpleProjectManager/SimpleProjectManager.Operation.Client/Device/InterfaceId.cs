using SimpleProjectManager.Shared;
using Vogen;

namespace SimpleProjectManager.Operation.Client.Device;

[ValueObject(typeof(string))]
[Instance("Empty", "")]
public readonly partial struct InterfaceId
{
    private static Validation Validate(string value)
        => value.ValidateNotNullOrEmpty(nameof(InterfaceId));
}