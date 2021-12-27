using System.Collections.Concurrent;
using JetBrains.Annotations;
using Tauron.Application.Workshop.Mutation;

namespace Tauron.Application.Workshop.StateManagement.DataFactorys;

[PublicAPI]
public class MapSourceFactory : AdvancedDataSourceFactory
{
    public ConcurrentDictionary<Type, Func<CreationMetadata?, object>> Map { get; private set; } = new();

    public void Register<TSource, TData>(Func<CreationMetadata?, TSource> factory)
        where TSource : IExtendedDataSource<TData>
    {
        Map[typeof(TData)] = m => factory(m);
    }

    public override bool CanSupply(Type dataType) => Map.ContainsKey(dataType);

    public override Func<IExtendedDataSource<TData>> Create<TData>(CreationMetadata? metadata)
    {
        if (Map.TryGetValue(typeof(TData), out var fac))
            return () => (IExtendedDataSource<TData>)fac(metadata);

        throw new InvalidOperationException("Not Supported Data Type Mapping");
    }
}