﻿using System;
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

        private static string GetCollectionDefaultName(Type targeType)
            => targeType.Name + "-Collection";

        private static string MakeKey(string databaseName, Type targetType)
            => GetCollectionDefaultName(targetType) + "-Mongo-DB-" + databaseName;

        private static string MakeSettingsKey(string databaseName, Type targetType)
            => GetCollectionDefaultName(targetType) + "-Mongo-DB-Settings-" + databaseName;

        public static void SetCustomCollectionName(CreationMetadata meta, Type targetType, string databaseName, string name) 
            => meta[MakeKey(databaseName, targetType)] = name;

        public static void SetCustomCollectionNameCreationSettings(CreationMetadata meta, Type targetType, string databaseName, CreateCollectionOptions options)
            => meta[MakeSettingsKey(databaseName, targetType)] = options;

        private readonly ImmutableDictionary<string, IMongoDatabase> _databases;

        public MongoDataBaseFactory(IEnumerable<MongoDatabase> databases)
            => _databases = databases.ToImmutableDictionary(database => database.Name, database => database.Database);

        public override Func<IExtendedDataSource<TData>> Create<TData>(CreationMetadata? metadata)
        {
            if (metadata == null || !metadata.TryGetValue(MongoDatabaseNameMeta, out var nameObject)
                                 || nameObject is not string name
                                 || !_databases.TryGetValue(name, out var database))
                throw new InvalidOperationException("Not Mongo Database Found");

            
            string collectionName = typeof(TData).Name + "-Collection";
            if (metadata.TryGetValue(MakeKey(name, typeof(TData)), out var collNameObj) && collNameObj is string collName)
                collectionName = collName;

            if (metadata.TryGetValue(MakeSettingsKey(name, typeof(TData)), out var settingsObj) && settingsObj is CreateCollectionOptions options)
            {
                if (!database.ListCollectionNames().Contains(s => s == collectionName))
                    database.CreateCollection(collectionName, options);
            }

            return () => new MongoDbSource<TData>(database.GetCollection<TData>(collectionName));
        }

        public override bool CanSupply(Type dataType) 
            => dataType.IsAssignableTo<IMongoEntity>();
    }
}