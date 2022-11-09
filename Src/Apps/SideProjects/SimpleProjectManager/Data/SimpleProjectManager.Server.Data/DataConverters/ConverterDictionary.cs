using System.Collections.Concurrent;

namespace SimpleProjectManager.Server.Data.DataConverters;

public sealed class ConverterDictionary
{
    private readonly ConcurrentDictionary<EntryKey, object> _converters = new();

    public Converter<TFrom, TTo> Get<TFrom, TTo>(Func<Converter<TFrom, TTo>> factory)
        => (Converter<TFrom, TTo>)_converters.GetOrAdd(
            new EntryKey(typeof(TFrom), typeof(TTo)),
            _ => factory());
}