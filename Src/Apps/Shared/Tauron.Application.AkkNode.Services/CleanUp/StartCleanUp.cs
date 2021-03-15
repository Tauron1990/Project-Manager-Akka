
using MongoDB.Driver;
using Tauron.Application.AkkaNode.Services.CleanUp.Core;
using Tauron.Application.AkkaNode.Services.MongoDb;
using Tauron.Application.Workshop.Mutation;
using Tauron.Application.Workshop.StateManagement;

namespace Tauron.Application.AkkaNode.Services.CleanUp
{
    public sealed record StartCleanUp : SimpleStateAction
    {
        private sealed record QueryInfo : MongoQueryBase<CleanUpTime>
        {
            public override FilterDefinition<CleanUpTime> Create() => Builders<CleanUpTime>.Filter.Empty;

            public override string ToHash() => nameof(CleanUpTime);
        }

        public override IQuery Query { get; } = new QueryInfo();
    }
}