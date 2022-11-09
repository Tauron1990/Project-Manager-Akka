using Vogen;

namespace SimpleProjectManager.Shared.Services.Devices;

[ValueObject(typeof(string))]
[Instance("Empty", "")]
public readonly partial struct DeviceName
{
    private static Validation Validate(string value)
        => string.IsNullOrWhiteSpace(value) ? Validation.Invalid("Name Should not be Empty") : Validation.Ok;
}