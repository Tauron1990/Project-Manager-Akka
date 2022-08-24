using Akkatecture.Aggregates;
using JetBrains.Annotations;
using SimpleProjectManager.Server.Core.Projections.Core;

namespace SimpleProjectManager.Server.Core.Projections;

[UsedImplicitly]
public class ProjectionModule : IModule
{

    public void Load(IServiceCollection collection)
    {
        /*collection.AddSingleton(
            c =>
            {
                var system = c.GetRequiredService<ActorSystem>();
                var database = system.Settings.Config.GetString("akka.persistence.journal.mongodb.connection-string");
                var url = MongoUrl.Create(database);

                return new MongoClient(url).GetDatabase(url.DatabaseName);
            })*/;
        collection.AddTransient(c => c.GetRequiredService<IEventAggregator>().GetEvent<DomainEventDispatcher, IDomainEvent>());
        
        collection.RegisterProjection<ProjectProjectionManager>();
    }
}