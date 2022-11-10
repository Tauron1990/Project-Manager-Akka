using System;

namespace Tauron.Application;

public sealed record PoolConfig<TToPool>(int MaximumSize, bool UseDispose, Func<TToPool> Factory)
{
    #pragma warning disable MA0018
    public static PoolConfig<TToPool> Default
        #pragma warning restore MA0018
        => new(
            0,
            UseDispose: true,
            () =>
            {
                if(FastReflection.Shared.GetCreator(typeof(TToPool), Type.EmptyTypes)?.Invoke(Array.Empty<object>()) is not TToPool data)
                    throw new InvalidOperationException("Pool Element could not be Created");

                return data;
            });
}