using System;
using System.Collections.Concurrent;
using JetBrains.Annotations;

namespace Tauron.Application;

[PublicAPI]
public sealed class ContextMetadata
{
    private const string DefaultKey = "Key";

    private readonly ConcurrentDictionary<CacheKey, object?> _meta = new();

    public TData Get<TData>(string name = DefaultKey)
        where TData : notnull
    {
        if(_meta.TryGetValue(new CacheKey(name, typeof(TData)), out object? dat) && dat is TData data)
            return data;

        throw new InvalidOperationException($"Not Metadata Found {name} -- {typeof(TData)}");
    }

    public TData? GetOptional<TData>(string name = DefaultKey)
    {
        if(_meta.TryGetValue(new CacheKey(name, typeof(TData)), out object? dat) && dat is TData data)
            return data;

        return default;
    }

    public void Set<TData>(string name, TData data)
        => _meta[new CacheKey(name, typeof(TData))] = data;

    public void Set<TData>(TData data)
        => _meta[new CacheKey(DefaultKey, typeof(TData))] = data;


    private sealed record CacheKey(string Name, Type Type);
}