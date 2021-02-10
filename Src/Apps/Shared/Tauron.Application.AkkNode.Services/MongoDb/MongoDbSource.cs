﻿using System;
using System.Threading.Tasks;
using MongoDB.Driver;
using Tauron.Application.Workshop.Mutation;

namespace Tauron.Application.AkkaNode.Services.MongoDb
{
    public sealed class MongoDbSource<TData> : IExtendedDataSource<TData>
    {
        private readonly IMongoCollection<TData> _data;

        public MongoDbSource(IMongoCollection<TData> data) => _data = data;

        public Task<TData> GetData(IQuery query)
        {
            return query switch
            {
                MongoQueryBase<TData> mongoQuery => _data.Find(mongoQuery.Create()).FirstOrDefaultAsync(),
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

                    _ => _data.ReplaceOneAsync(mongoQuery.Create(), data)
                },
                _ => throw new InvalidOperationException("Only Mongo Query with Correct Type are Valid (MongoQueryBase<TData>)")
            };
        }

        public Task OnCompled(IQuery query) => Task.CompletedTask;
    }
}