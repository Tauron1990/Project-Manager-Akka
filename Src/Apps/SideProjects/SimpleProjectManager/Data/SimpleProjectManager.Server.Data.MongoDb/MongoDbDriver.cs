using Akka.Actor;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Tauron.AkkaHost;

namespace SimpleProjectManager.Server.Data.MongoDb;

public class MongoDbDriver : IDatabaseModule
{
    public void Configurate(IActorApplicationBuilder builder)
    {
        BsonClassMap.RegisterClassMap<CheckPointInfo>(
            b =>
            {
                b.AutoMap();
                b.MapIdProperty(cp => cp.Id)
                   .SetIdGenerator(StringObjectIdGenerator.Instance)
                   .SetSerializer(new StringSerializer(BsonType.ObjectId));
            });
        
        builder.ConfigureServices(
            (_, coll) => coll
               .AddSingleton(
                    c =>
                    {
                        var system = c.GetRequiredService<ActorSystem>();
                        var database = system.Settings.Config.GetString("akka.persistence.journal.mongodb.connection-string");
                        var url = MongoUrl.Create(database);

                        return new MongoClient(url).GetDatabase(url.DatabaseName);
                    })
               .AddScoped(c => new GridFSBucket(c.GetRequiredService<IMongoDatabase>()))
               .AddSingleton<IInternalDataRepository, InternalDataRepository>()
               .AddSingleton<IInternalFileRepository, InternalFileRepository>());
    }
}