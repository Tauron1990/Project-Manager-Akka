using Vogen;

namespace SimpleProjectManager.Shared.ServerApi;

[ValueObject(typeof(string))]
[Instance("IsOk", "Ok")]
#pragma warning disable MA0097
public readonly partial struct PingResult
    #pragma warning restore MA0097
{
    private static Validation Validate(string value)
        => Validation.Invalid("Only One Static Correct Value");
}