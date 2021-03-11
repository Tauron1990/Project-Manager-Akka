using System;
using Autofac;
using JetBrains.Annotations;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Tauron.Application.AkkaNode.Services.CleanUp.Core;
using Tauron.Application.AkkaNode.Services.MongoDb;
using Tauron.Application.Workshop.StateManagement;
using Tauron.Application.Workshop.StateManagement.DataFactorys;

namespace Tauron.Application.AkkaNode.Services.CleanUp
{
    [PublicAPI]
    public static class CleanUpRegistration
    {
        public static ManagerBuilder AddCleanUp(this ManagerBuilder builder, string databaseName, string collectionName, string revisionName, GridFSBucket bucket)
        {
            if (builder.ComponentContext == null)
                throw new InvalidOperationException("Need Component Context from Autofac");

            CreationMetadata meta = new() {[MongoDataBaseFactory.MongoDatabaseNameMeta] = databaseName};

            MongoDataBaseFactory.SetCustomCollectionName(meta, typeof(CleanUpTime), databaseName, collectionName);
            MongoDataBaseFactory.SetCustomCollectionNameCreationSettings(meta, typeof(CleanUpTime), databaseName, new CreateCollectionOptions {Capped = true, MaxDocuments = 1, MaxSize = 1024});

            MongoDataBaseFactory.SetCustomCollectionName(meta, typeof(ToDeleteRevision), databaseName, revisionName);


            var data = MergeFactory.Merge(new BuckedSourceFactory(bucket), builder.ComponentContext.Resolve<MongoDataBaseFactory>());

            return builder.AddTypes(data, meta, typeof(CleanUpManager), typeof(CleanUpReducer));
        }
    }
}