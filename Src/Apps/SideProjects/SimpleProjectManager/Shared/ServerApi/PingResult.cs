using Vogen;

namespace SimpleProjectManager.Shared.ServerApi;

[ValueObject(typeof(string))]
[Instance("IsOk", "Ok")]
public readonly partial struct PingResult
{
    private static Validation Validate(string value)
        => Validation.Invalid("Only One Static Correct Value");
}