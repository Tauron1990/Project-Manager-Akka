using Akkatecture.Core;

namespace Tauron.Applicarion.Redux.Extensions.Cache;

#pragma warning disable MA0097
public sealed class CacheTimeoutId : Identity<CacheTimeoutId>
    #pragma warning restore MA0097
{
    private static readonly Guid Namespace = new("99956BD0-600D-4413-B7D6-C133B982D95D");
    public CacheTimeoutId(string value) : base(value) { }

    public static CacheTimeoutId FromCacheId(CacheDataId id)
        => NewDeterministic(Namespace, id.Value);
}