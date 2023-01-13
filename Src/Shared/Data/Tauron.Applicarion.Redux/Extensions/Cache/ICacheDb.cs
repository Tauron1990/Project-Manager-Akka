using JetBrains.Annotations;

namespace Tauron.Applicarion.Redux.Extensions.Cache;

[PublicAPI]
public interface ICacheDb
{
    ValueTask DeleteElement(CacheTimeoutId key);

    ValueTask DeleteElement(CacheDataId key);

    ValueTask<(CacheTimeoutId? id, CacheDataId? Key, DateTime Time)> GetNextTimeout();

    //ValueTask UpdateTimeout(CacheDataId key);

    ValueTask TryAddOrUpdateElement(CacheDataId key, string data);

    ValueTask<string?> ReNewAndGet(CacheDataId key);
}