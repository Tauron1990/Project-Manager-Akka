using System;
using System.Collections.Immutable;

namespace Tauron.Application;

[PublicAPI]
public sealed class ContextMetadata
{
    private const string DefaultKey = "Key";

    private readonly ImmutableDictionary<CacheKey, object?> _meta;

    public ContextMetadata() => _meta = ImmutableDictionary<CacheKey, object?>.Empty;

    private ContextMetadata(ImmutableDictionary<CacheKey, object?> meta) => _meta = meta;

    [Pure]
    public Result<TData> Get<TData>(string name = DefaultKey)
        where TData : notnull
    {
        if(_meta.TryGetValue(new CacheKey(name, typeof(TData)), out object? dat) && dat is TData data)
            return data;
        
        return Result.Error<TData>(new InvalidOperationException($"Not Metadata Found {name} -- {typeof(TData)}"));
    }

    [Pure]
    public Option<TData> GetOptional<TData>(string name = DefaultKey)
    {
        if(_meta.TryGetValue(new CacheKey(name, typeof(TData)), out object? dat) && dat is TData data)
            return data;

        return Option<TData>.None;
    }

    [Pure]
    public ContextMetadata Set<TData>(string name, TData data) =>
        new(_meta.SetItem(new CacheKey(name, typeof(TData)), data));

    [Pure]
    public ContextMetadata Set<TData>(TData data) =>
        new(_meta.SetItem(new CacheKey(DefaultKey, typeof(TData)), data));

    private sealed record CacheKey(string Name, Type Type);
}