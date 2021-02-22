using System;
using FluentValidation;
using JetBrains.Annotations;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Tauron.Application.AkkaNode.Services.CleanOld;
using Tauron.Application.AkkaNode.Services.CleanUp.Core;
using Tauron.Application.AkkaNode.Services.MongoDb;
using Tauron.Application.Workshop.StateManagement;

namespace Tauron.Application.AkkaNode.Services.CleanUp
{
    [PublicAPI]
    public static class CleanUpRegistration
    {
        public static ManagerBuilder AddCleanUp(this ManagerBuilder builder, string databaseName, string collectionName, string revisionName)
        {
            if (builder.ComponentContext == null)
                throw new InvalidOperationException("Need Component Context from Autofac");

            builder.WithConsistentHashDispatcher();

            CreationMetadata meta = new();

            MongoDataBaseFactory.SetCustomCollectionName(meta, typeof(CleanUpTime), databaseName, collectionName);
            MongoDataBaseFactory.SetCustomCollectionNameCreationSettings(meta, typeof(CleanUpTime), databaseName, new CreateCollectionOptions {Capped = true, MaxDocuments = 1, MaxSize = 1024});

            MongoDataBaseFactory.SetCustomCollectionName(meta, typeof(ToDeleteRevision), databaseName, revisionName);

            return builder.AddMongoFromAssembly(builder.ComponentContext, databaseName, meta, typeof(CleanUpManager), typeof(CleanUpReducer));
        }
    }
}