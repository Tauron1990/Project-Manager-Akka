using Stl.Fusion.Swapping;

namespace SimpleProjectManager.Client.Data.Cache;

public class IndexedDdSwapService : SwapServiceBase
{
    protected override ValueTask<string?> Load(string key, CancellationToken cancellationToken)
        => throw new NotImplementedException();

    protected override ValueTask<bool> Renew(string key, CancellationToken cancellationToken)
        => throw new NotImplementedException();

    protected override ValueTask Store(string key, string value, CancellationToken cancellationToken)
        => throw new NotImplementedException();
}