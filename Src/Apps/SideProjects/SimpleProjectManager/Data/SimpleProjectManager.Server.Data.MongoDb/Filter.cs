using MongoDB.Driver;

namespace SimpleProjectManager.Server.Data.MongoDb;

public sealed record Filter<TData>(FilterDefinition<TData> FilterDefinition) : IFilter<TData>
{
    public IFilter<TData> Not => new Filter<TData>(Builders<TData>.Filter.Not(FilterDefinition));
}