using MongoDB.Driver;
using Tauron.Application.AkkaNode.Services.MongoDb;
using Tauron.Application.Workshop.StateManagement;

namespace Tauron.Application.AkkaNode.Services.CleanUp.Core
{
    public sealed record InitializeCleanUpAction : SimpleStateAction
    {
        private sealed record InitializeQuery : MongoQueryBase<CleanUpTime>
        {
            public override FilterDefinition<CleanUpTime> Create() => FilterDefinition<CleanUpTime>.Empty;

            public override string ToHash() => nameof(InitializeCleanUpAction);
        }
    }
}