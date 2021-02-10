﻿using Autofac;
using JetBrains.Annotations;
using MongoDB.Driver;
using Tauron.Application.Workshop.StateManagement;

namespace Tauron.Application.AkkaNode.Services.MongoDb
{
    [PublicAPI]
    public static class MongoDbExtensions
    {
        public static void AddMongoDb(this ContainerBuilder builder, MongoUrl url, string database)
        {
            if(builder.Properties.ContainsKey("MongoDd-Database-" + database)) return;

            builder.Properties.Add("MongoDd-Database-" + database, null);
            builder.WhenNotRegistered<IClientPool>(b => b.RegisterType<ClientPool>().As<IClientPool>().SingleInstance());
            builder.WhenNotRegistered<MongoDataBaseFactory>(b => b.RegisterType<MongoDataBaseFactory>().AsSelf().SingleInstance());

            builder.Register(ctx => new MongoDatabase(ctx.Resolve<IClientPool>().Get(url).GetDatabase(database), database)).AsSelf().SingleInstance();
        }

        public static ManagerBuilder AddMongoFromAssembly(this ManagerBuilder builder, IComponentContext context)
            => builder.AddFromAssembly(context.Resolve<MongoDataBaseFactory>())
    }
}