using MongoDB.Driver;
using Tauron.Application.AkkaNode.Services.MongoDb;
using Tauron.Application.Workshop.Mutation;
using Tauron.Application.Workshop.StateManagement;

namespace Tauron.Application.AkkaNode.Services.CleanUp.Core
{
    public sealed record RunCleanUpAction : SimpleStateAction
    {
        private sealed record CollectDataQuery : MongoQueryBase<ToDeleteRevision>
        {
            public override FilterDefinition<ToDeleteRevision> Create() => Builders<ToDeleteRevision>.Filter.Empty;

            public override string ToHash() => "CleanUp-CollectData";
        }

        public override IQuery Query { get; } = new CollectDataQuery();
    }
}