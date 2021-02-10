using System;
using Tauron.Application.Workshop.Mutation;

namespace Tauron.Application.Workshop.StateManagement.DataFactorys
{
    public abstract class AdvancedDataSourceFactory : IDataSourceFactory
    {
        public abstract Func<IExtendedDataSource<TData>> Create<TData>(CreationMetadata? metadata)
            where TData : class, IStateEntity;

        public abstract bool CanSupply(Type dataType);
    }
}