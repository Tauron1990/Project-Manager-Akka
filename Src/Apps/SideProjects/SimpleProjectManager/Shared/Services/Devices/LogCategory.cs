using Vogen;

namespace SimpleProjectManager.Shared.Services.Devices;

[ValueObject(typeof(string))]
public readonly partial struct LogCategory
{
    private static Validation Validate(string value)
        => value.ValidateNotNullOrEmpty(nameof(LogCategory));
}