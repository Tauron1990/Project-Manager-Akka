using System.Linq.Expressions;
using MongoDB.Driver;

namespace SimpleProjectManager.Server.Data.MongoDb;

public sealed class OperationFactory<TData> : IOperationFactory<TData>
{
    public IFilter<TData> Empty { get; } = new Filter<TData>(Builders<TData>.Filter.Empty);

    public IFilter<TData> Eq<TField>(Expression<Func<TData, TField>> selector, TField toEqual)
        => new Filter<TData>(Builders<TData>.Filter.Eq(selector, toEqual));

    public IFilter<TData> Or(params IFilter<TData>[] filter)
        => new Filter<TData>(Builders<TData>.Filter.Or(filter.Cast<Filter<TData>>().Select(f => f.FilterDefinition)));

    public IUpdate<TData> Set<TField>(Expression<Func<TData, TField>> selector, TField value)
        => new Updater<TData>(Builders<TData>.Update.Set(selector, value));
}