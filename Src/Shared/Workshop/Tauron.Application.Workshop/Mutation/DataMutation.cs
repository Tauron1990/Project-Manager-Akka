using System;
using JetBrains.Annotations;

namespace Tauron.Application.Workshop.Mutation;

[PublicAPI]
// ReSharper disable once UnusedTypeParameter
public sealed class DataMutation<TData> : ISyncMutation
    where TData : class
{
    private readonly object? _hash;
    private readonly Action _task;

    public DataMutation(Action task, string name, object? hash = null)
    {
        _task = task;
        _hash = hash;
        Name = name;
    }

    public object ConsistentHashKey => _hash ?? Name;

    public string Name { get; }
    Action ISyncMutation.Run => _task;
}