using JetBrains.Annotations;
using Microsoft.Extensions.Caching.Memory;

namespace SimpleProjectManager.Server.Core.Data;

[PublicAPI]
public sealed class RequestCache<TResult>
{
    private readonly IMemoryCache _memoryCache;
    private readonly TimeSpan _timeOut = TimeSpan.FromMinutes(1);

    public RequestCache(IMemoryCache memoryCache)
        => _memoryCache = memoryCache;

    public async ValueTask<TResult?> GetResult(string key)
    {
        if(_memoryCache.TryGetValue<Task<TResult>>(key, out var result) && result is not null)
            return await result.ConfigureAwait(false);

        return default;
    }

    public string PostRequest(Task<TResult> result)
    {
        var key = Guid.NewGuid().ToString("N");
        _memoryCache.Set(key, result, _timeOut).Ignore();

        return key;
    }
}