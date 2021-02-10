using MongoDB.Driver;

namespace Tauron.Application.AkkaNode.Services.MongoDb
{
    public interface IMongoUpdateable<TData>
    {
        bool Delete { get; }

        UpdateDefinition<TData> CreateUpdate();
    }
}