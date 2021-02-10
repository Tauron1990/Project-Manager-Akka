using MongoDB.Driver;

namespace Tauron.Application.AkkaNode.Services.MongoDb
{
    public sealed record MongoDatabase(IMongoDatabase Database, string Name);
}