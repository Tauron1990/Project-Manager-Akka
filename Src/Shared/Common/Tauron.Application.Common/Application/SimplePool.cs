using System;
using System.Collections.Immutable;

namespace Tauron.Application;

[PublicAPI]
public sealed class SimplePool<TToPool>
{
    private readonly ImmutableQueue<TToPool> _pool;
    private readonly PoolConfig<TToPool> _config;

    public SimplePool(PoolConfig<TToPool> config)
    {
        _pool = ImmutableQueue<TToPool>.Empty;
        _config = config;
    }

    public SimplePool(ImmutableQueue<TToPool> pool, PoolConfig<TToPool> config, int count)
    {
        _pool = pool;
        _config = config;
        Count = count;
    }

    public int Count { get; }

    public bool IsFull => _config.MaximumSize > 0 && Count > _config.MaximumSize;
    
    [Pure]
    public SimplePool<TToPool> RefConfig(Func<PoolConfig<TToPool>, PoolConfig<TToPool>> configChange)
    {
        var newConfig = configChange(_config);
        int count = Count;
        var queue = _pool;

        while (count > newConfig.MaximumSize)
        {
            queue = queue.Dequeue();
            count = count - 1;
        }

        return new SimplePool<TToPool>(queue, newConfig, count);
    }

    [Pure]
    public (TToPool Element, SimplePool<TToPool> Poll) Rent()
    {
        if(_pool.IsEmpty)
            return (_config.Factory(), this);

        var pool = _pool.Dequeue(out TToPool? ele);
        return (ele, new SimplePool<TToPool>(pool, _config, Count - 1));
    }

    [Pure]
    public SimplePool<TToPool> Return(TToPool pooledObject)
    {
        if(pooledObject is IDisposable disposable)
            disposable.Dispose();
        
        return IsFull 
            ? this 
            : new SimplePool<TToPool>(_pool.Enqueue(pooledObject), _config, Count + 1);

    }

    [Pure]
    public SimplePool<TToPool> Clear() => new(_pool.Clear(), _config, 0);
}