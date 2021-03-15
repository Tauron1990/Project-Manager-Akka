using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Tauron.Application.AkkaNode.Services.MongoDb;
using Tauron.Application.Workshop.StateManagement;

namespace Tauron.Application.AkkaNode.Services.CleanUp.Core
{
    public sealed record ToDeleteRevision(ObjectId Id, string BuckedId, [property: BsonIgnore] bool IsChanged = false, [property:BsonIgnore] bool Delete = false) 
        : IChangeTrackable, IMongoEntity, IMongoUpdateable<ToDeleteRevision>
    {
        public UpdateDefinition<ToDeleteRevision> CreateUpdate() => throw new InvalidOperationException("Updates Not Supported");

        public ToDeleteRevision MarkForDelete() => this with {IsChanged = true, Delete = true};
    }
}