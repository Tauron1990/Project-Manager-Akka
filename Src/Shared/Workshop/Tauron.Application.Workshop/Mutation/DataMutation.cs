using System;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Tauron.Application.Workshop.Mutation;

public interface IDataMutation
{
    string Name { get; }
}

public interface ISyncMutation : IDataMutation
{
    Action Run { get; }
}

public interface IAsyncMutation : IDataMutation
{
    Func<Task> Run { get; }
}

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

[PublicAPI]
// ReSharper disable once UnusedTypeParameter
public sealed class AsyncDataMutation<TData> : IAsyncMutation
    where TData : class
{
    private readonly object? _hash;
    private readonly Func<Task> _task;

    public AsyncDataMutation(Func<Task> task, string name, object? hash = null)
    {
        _task = task;
        _hash = hash;
        Name = name;
    }

    public object ConsistentHashKey => _hash ?? Name;

    public string Name { get; }
    Func<Task> IAsyncMutation.Run => _task;
}