using MongoDB.Bson;

namespace Tauron.Application.AkkaNode.Services.CleanUp
{
    public sealed record ToDeleteRevision(ObjectId Id, string BuckedId)
    {
        public ToDeleteRevision(string buckedId)  
            : this(ObjectId.Empty, buckedId)
        {
            
        }
    }
}