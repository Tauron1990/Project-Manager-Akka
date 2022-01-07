using JetBrains.Annotations;

namespace Tauron.Applicarion.Redux.Extensions.Cache;

[PublicAPI]
public interface ICacheDb
{
    Task DeleteElement(CacheTimeoutId key);

    Task DeleteElement(CacheDataId key);

    Task<(CacheTimeoutId? id, CacheDataId? Key, DateTime Time)> GetNextTimeout();

    Task UpdateTimeout(CacheDataId key);

    Task TryAddOrUpdateElement(CacheDataId key, string data);

    Task<string?> ReNewAndGet(CacheDataId key);
} 