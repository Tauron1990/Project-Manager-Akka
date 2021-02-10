using System.Collections.Concurrent;
using MongoDB.Driver;

namespace Tauron.Application.AkkaNode.Services.MongoDb
{
    public sealed class ClientPool : IClientPool
    {
        private readonly ConcurrentDictionary<MongoUrl, IMongoClient> _pool = new();


        public IMongoClient Get(MongoUrl url) => _pool.GetOrAdd(url, mongoUrl => new MongoClient(mongoUrl));
    }
}