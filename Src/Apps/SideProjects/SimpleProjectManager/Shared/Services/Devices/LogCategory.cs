using Vogen;

namespace SimpleProjectManager.Shared.Services.Devices;

[ValueObject<string>]
public partial struct LogCategory
{
    private static Validation Validate(string value)
        => value.ValidateNotNullOrEmpty(nameof(LogCategory));
}