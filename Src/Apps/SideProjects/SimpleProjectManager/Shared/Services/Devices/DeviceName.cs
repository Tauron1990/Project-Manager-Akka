using Vogen;

namespace SimpleProjectManager.Shared.Services.Devices;

[ValueObject(typeof(string))]
[Instance("Empty", "")]
#pragma warning disable MA0097
public readonly partial struct DeviceName
    #pragma warning restore MA0097
{
    private static Validation Validate(string value)
        => string.IsNullOrWhiteSpace(value) ? Validation.Invalid("Name Should not be Empty") : Validation.Ok;
}