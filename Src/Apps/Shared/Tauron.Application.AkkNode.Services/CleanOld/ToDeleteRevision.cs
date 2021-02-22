using MongoDB.Bson;
using Tauron.Application.AkkaNode.Services.MongoDb;

namespace Tauron.Application.AkkaNode.Services.CleanOld
{
    public sealed record ToDeleteRevision(ObjectId Id, string BuckedId) : IMongoEntity
    {
        public ToDeleteRevision(string buckedId)
            : this(ObjectId.Empty, buckedId)
        {
        }
    }
}