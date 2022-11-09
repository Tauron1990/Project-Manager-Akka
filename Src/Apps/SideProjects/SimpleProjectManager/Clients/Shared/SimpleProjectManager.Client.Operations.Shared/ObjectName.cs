using SimpleProjectManager.Shared;
using Vogen;

namespace SimpleProjectManager.Client.Operations.Shared;

[ValueObject(typeof(string))]
[Instance("Empty", "")]
public readonly partial struct ObjectName
{
    private static Validation Validate(string value)
        => value.ValidateNotNullOrEmpty(nameof(ObjectName));
}