using MongoDB.Bson;
using Tauron.Application.AkkaNode.Services.MongoDb;

namespace Tauron.Application.AkkaNode.Services.CleanUp.Core
{
    public sealed record ToDeleteRevision(ObjectId Id, string BuckedId) : IMongoEntity
    {
        public ToDeleteRevision(string buckedId)
            : this(ObjectId.Empty, buckedId)
        {
        }
    }
}