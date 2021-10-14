using System;
using System.Collections.Concurrent;

namespace Test.Helper;

public sealed class DeterministicServiceProvider : IServiceProvider
{
    private readonly ConcurrentDictionary<Type, object> _objects = new();

    public DeterministicServiceProvider(params (Type, object)[] instances)
    {
        foreach (var (serviceType, instance) in instances)
        {
            _objects.TryAdd(serviceType, instance);
        }
    }

    public object? GetService(Type serviceType)
        => _objects.TryGetValue(serviceType, out var value) ? value : null;
}