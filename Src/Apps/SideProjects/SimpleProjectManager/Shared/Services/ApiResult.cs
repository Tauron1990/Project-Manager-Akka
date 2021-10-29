using Tauron.Operations;

namespace SimpleProjectManager.Shared.Services;

public sealed record ApiResult(string? Error)
{
    public ApiResult(IOperationResult result)
        : this(result.Ok ? null : result.Error)
    {

    }

    public bool IsValid => string.IsNullOrWhiteSpace(Error);
}