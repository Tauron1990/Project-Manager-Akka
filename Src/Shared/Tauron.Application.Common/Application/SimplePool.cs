using System;
using System.Collections.Concurrent;
using JetBrains.Annotations;

namespace Tauron.Application;

public sealed record PoolConfig<TToPool>(int MaximumSize, bool UseDispose, Func<TToPool> Factory)
{
    public static PoolConfig<TToPool> Default 
        => new(0, UseDispose: true, () => (TToPool)FastReflection.Shared.GetCreator(typeof(TToPool), Type.EmptyTypes)(Array.Empty<object>()));
}

[PublicAPI]
public sealed class SimplePool<TToPool>
{
    private PoolConfig<TToPool> _config;
    private readonly ConcurrentQueue<TToPool> _pool = new();

    public int Count => _pool.Count;

    public bool IsFull => _config.MaximumSize > 0 && _pool.Count > _config.MaximumSize;
    
    public SimplePool(PoolConfig<TToPool> config)
        => _config = config;

    public void RefConfig(Func<PoolConfig<TToPool>, PoolConfig<TToPool>> configChange)
    {
        _config = configChange(_config);

        if (_config.MaximumSize <= 0 || _pool.IsEmpty || _pool.Count < _config.MaximumSize) return;

        do
        {
            if (!_pool.TryDequeue(out _))
                break;
        } while (_pool.Count > _config.MaximumSize);
    }

    public TToPool Rent()
        => _pool.TryDequeue(out var pooledObject) ? pooledObject : _config.Factory();

    public void Return(TToPool pooledObject)
    {
        if(_config.UseDispose && pooledObject is IDisposable disposable)
                disposable.Dispose();
        
        if(IsFull) return;
        _pool.Enqueue(pooledObject);
    }

    public void Clear()
        => _pool.Clear();
}