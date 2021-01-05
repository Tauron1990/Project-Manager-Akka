using System;
using System.Collections.Concurrent;
using JetBrains.Annotations;
using Tauron.Application.Workshop.Mutation;

namespace Tauron.Application.Workshop.StateManagement.DataFactorys
{
    [PublicAPI]
    public class MapSourceFactory : AdvancedDataSourceFactory
    {
        public ConcurrentDictionary<Type, Func<object>> Map { get; private set; } = new();

        public void Register<TSource, TData>(Func<TSource> factory)
            where TSource : IExtendedDataSource<TData>
            => Map[typeof(TData)] = () => factory();

        public override bool CanSupply(Type dataType) => Map.ContainsKey(dataType);

        public override Func<IExtendedDataSource<TData>> Create<TData>()
        {
            if (Map.TryGetValue(typeof(TData), out var fac))
                return () => (IExtendedDataSource<TData>) fac();

            throw new InvalidOperationException("Not Supported Data Type Mapping");
        }
    }
}