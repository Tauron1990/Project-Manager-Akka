using SimpleProjectManager.Shared;
using Vogen;

namespace SimpleProjectManager.Operation.Client.Device.Dummy;

[ValueObject(typeof(string))]
public readonly partial struct CategoryName
{
    private static Validation Validate(string value)
        => value.ValidateNotNullOrEmpty(nameof(CategoryName));
}