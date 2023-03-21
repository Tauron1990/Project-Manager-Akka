using System;

namespace Tauron.Application;

[PublicAPI]
public sealed record PoolConfig<TToPool>(int MaximumSize, bool UseDispose, Func<TToPool> Factory)
{
    #pragma warning disable MA0018
    [Pure]
    public static PoolConfig<TToPool> Default(Func<TToPool> factory)
        => new(0, UseDispose: true, factory);
}