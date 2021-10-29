﻿using Akka.Actor;
using Akkatecture.Aggregates;
using Autofac;
using MongoDB.Driver;
using SimpleProjectManager.Server.Core.Projections.Core;
using Tauron.Application;
using Tauron.Application.AkkaNode.Bootstrap;

namespace SimpleProjectManager.Server.Core.Projections;

public class ProjectionModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.Register(
            c =>
            {
                var system = c.Resolve<ActorSystem>();
                var database = system.Settings.Config.GetString("akka.persistence.journal.mongodb.connection-string");
                var url = MongoUrl.Create(database);

                return new MongoClient(url).GetDatabase(url.DatabaseName);
            }).SingleInstance();
        builder.Register(c => c.Resolve<IEventAggregator>().GetEvent<DomainEventDispatcher, IDomainEvent>());
        builder.RegisterType<InternalRepository>();
        builder.RegisterStartUpAction<ProjectionInitializer>();

        builder.RegisterProjection<ProjectProjectionManager>();

        base.Load(builder);
    }
}