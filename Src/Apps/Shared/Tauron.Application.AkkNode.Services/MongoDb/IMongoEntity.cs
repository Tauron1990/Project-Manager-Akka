using MongoDB.Bson;

namespace Tauron.Application.AkkaNode.Services.MongoDb
{
    public interface IMongoEntity
    {
        ObjectId Id { get; }
    }
}