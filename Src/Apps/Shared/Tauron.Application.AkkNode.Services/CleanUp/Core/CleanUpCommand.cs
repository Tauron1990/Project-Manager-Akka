using MongoDB.Driver;
using Tauron.Application.AkkaNode.Services.MongoDb;
using Tauron.Application.Workshop.StateManagement;

namespace Tauron.Application.AkkaNode.Services.CleanUp.Core
{
    public abstract record CleanUpCommand<TData> : MongoQueryBase<TData>
    {
        public abstract FilterDefinition<TData> Create();
        public abstract string ToHash();
    }
}