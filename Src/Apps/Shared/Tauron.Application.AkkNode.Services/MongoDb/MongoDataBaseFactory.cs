using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Autofac;
using MongoDB.Driver;
using Tauron.Application.Workshop.Mutation;
using Tauron.Application.Workshop.StateManagement;
using Tauron.Application.Workshop.StateManagement.DataFactorys;

namespace Tauron.Application.AkkaNode.Services.MongoDb
{
    public sealed class MongoDataBaseFactory : AdvancedDataSourceFactory
    {
        public const string MongoDatabaseNameMeta = nameof(MongoDatabaseNameMeta);

        private readonly ImmutableDictionary<string, IMongoDatabase> _databases;

        public MongoDataBaseFactory(IEnumerable<MongoDatabase> databases)
            => _databases = databases.ToImmutableDictionary(database => database.Name, database => database.Database);

        public override Func<IExtendedDataSource<TData>> Create<TData>(CreationMetadata? metadata)
        {
            if (metadata == null || !metadata.TryGetValue(MongoDatabaseNameMeta, out var nameObject)
                                 || nameObject is not string name
                                 || !_databases.TryGetValue(name, out var database))
                throw new InvalidOperationException("Not Mongo Database Found");

            return () => new MongoDbSource<TData>(database.GetCollection<TData>(typeof(TData).Name + "-Collection"));
        }

        public override bool CanSupply(Type dataType) 
            => dataType.IsAssignableTo<IMongoEntity>();
    }
}