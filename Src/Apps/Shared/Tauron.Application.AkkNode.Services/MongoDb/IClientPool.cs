using MongoDB.Driver;

namespace Tauron.Application.AkkaNode.Services.MongoDb
{
    public interface IClientPool
    {
        IMongoClient Get(MongoUrl url);
    }
}