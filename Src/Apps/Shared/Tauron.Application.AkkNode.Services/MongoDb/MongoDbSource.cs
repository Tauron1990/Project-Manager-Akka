using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Akka.Util.Internal;
using MongoDB.Driver;
using Tauron.Application.Workshop.Mutation;

namespace Tauron.Application.AkkaNode.Services.MongoDb
{
    public sealed class MongoDbSource<TData> : IExtendedDataSource<TData>
    {
        private readonly IMongoCollection<TData> _data;

        public MongoDbSource(IMongoCollection<TData> data) => _data = data;

        public async Task<TData> GetData(IQuery query)
        {
            return query switch
            {
                MongoQueryBase<TData> mongoQuery =>  await _data.Find(mongoQuery.Create()).SingleOrDefaultAsync(),
                _ => throw new InvalidOperationException("Only Mongo Query with Correct Type are Valid (MongoQueryBase<TData>)")
            };
        }

        public Task SetData(IQuery query, TData data)
        {
            return query switch
            {
                MongoQueryBase<TData> mongoQuery => data switch
                {
                    IMongoUpdateable<TData> update => update.Delete 
                        ? _data.DeleteOneAsync(mongoQuery.Create()) 
                        : _data.UpdateOneAsync(mongoQuery.Create(), update.CreateUpdate()),

                    _ => _data.ReplaceOneAsync(mongoQuery.Create(), data, new ReplaceOptions{ IsUpsert = true})
                },
                _ => throw new InvalidOperationException("Only Mongo Query with Correct Type are Valid (MongoQueryBase<TData>)")
            };
        }

        public Task OnCompled(IQuery query) => Task.CompletedTask;
    }

    public sealed class MongoDbListSource<TData> : IExtendedDataSource<ImmutableList<TData>>
    {
        private readonly IMongoCollection<TData> _data;

        public MongoDbListSource(IMongoCollection<TData> data) => _data = data;

        public async Task<ImmutableList<TData>> GetData(IQuery query)
        {
            return query switch
            {
                MongoQueryBase<TData> mongoQuery => ImmutableList.CreateRange(await _data.Find(mongoQuery.Create()).ToListAsync()),
                _ => throw new InvalidOperationException("Only Mongo Query with Correct Type are Valid (MongoQueryBase<TData>)")
            };
        }

        public Task SetData(IQuery query, ImmutableList<TData> dataList)
        {
            return query switch
            {
                MongoQueryBase<TData> mongoQuery =>
                    ForEach(dataList, data => data switch
                    {
                        IMongoUpdateable<TData> update => update.Delete
                            ? _data.DeleteOneAsync(mongoQuery.Create())
                            : _data.UpdateOneAsync(mongoQuery.Create(), update.CreateUpdate()),

                        _ => _data.ReplaceOneAsync(mongoQuery.Create(), data, new ReplaceOptions {IsUpsert = true})
                    }),
                _ => throw new InvalidOperationException("Only Mongo Query with Correct Type are Valid (MongoQueryBase<TData>)")
            };
        }

        private async Task ForEach(ImmutableList<TData> list, Func<TData, Task> dataHandler)
        {
            foreach (var data in list) await dataHandler(data);
        }

        public Task OnCompled(IQuery query) => Task.CompletedTask;
    }
}