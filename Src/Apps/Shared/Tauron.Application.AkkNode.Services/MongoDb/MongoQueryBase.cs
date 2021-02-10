using MongoDB.Driver;
using Tauron.Application.Workshop.Mutation;

namespace Tauron.Application.AkkaNode.Services.MongoDb
{
    public abstract record MongoQueryBase<TData> : IQuery
    {
        public abstract FilterDefinition<TData> Create();
        public abstract string ToHash();
    }
}