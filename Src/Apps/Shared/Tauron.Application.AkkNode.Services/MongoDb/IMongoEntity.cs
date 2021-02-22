using JetBrains.Annotations;
using MongoDB.Bson;
using Tauron.Application.Workshop.StateManagement;

namespace Tauron.Application.AkkaNode.Services.MongoDb
{
    public interface IMongoEntity : IStateEntity
    {
        [UsedImplicitly]
        ObjectId Id { get; }
    }
}