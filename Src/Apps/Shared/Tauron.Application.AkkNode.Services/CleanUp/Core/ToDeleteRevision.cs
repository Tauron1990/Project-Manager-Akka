using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Tauron.Application.AkkaNode.Services.MongoDb;
using Tauron.Application.Workshop.StateManagement;

namespace Tauron.Application.AkkaNode.Services.CleanUp.Core
{
    public sealed record ToDeleteRevision(ObjectId Id, string BuckedId, [property: BsonIgnore] bool IsChanged = false) : IChangeTrackable, IMongoEntity;
}