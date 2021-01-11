using MongoDB.Bson;

namespace Tauron.Application.AkkNode.Services.CleanUp
{
    public sealed record ToDeleteRevision(ObjectId Id, string BuckedId);
}