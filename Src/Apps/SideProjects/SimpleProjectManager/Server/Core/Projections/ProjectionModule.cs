using Akka.Actor;
using Akkatecture.Aggregates;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using SimpleProjectManager.Server.Core.Projections.Core;
using Tauron.Application.AkkaNode.Bootstrap;

namespace SimpleProjectManager.Server.Core.Projections;

public class ProjectionModule : IModule
{

    public void Load(IServiceCollection collection)
    {
        collection.AddSingleton(
            c =>
            {
                var system = c.GetRequiredService<ActorSystem>();
                var database = system.Settings.Config.GetString("akka.persistence.journal.mongodb.connection-string");
                var url = MongoUrl.Create(database);

                return new MongoClient(url).GetDatabase(url.DatabaseName);
            });
        collection.AddScoped(c => new GridFSBucket(c.GetRequiredService<IMongoDatabase>()));

        collection.AddTransient(c => c.GetRequiredService<IEventAggregator>().GetEvent<DomainEventDispatcher, IDomainEvent>());
        collection.AddScoped<InternalDataRepository>();

        collection.RegisterProjection<ProjectProjectionManager>();
    }
}