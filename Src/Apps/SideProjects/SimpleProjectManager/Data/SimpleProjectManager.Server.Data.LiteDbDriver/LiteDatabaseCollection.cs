using LiteDB;
using LiteDB.Async;

namespace SimpleProjectManager.Server.Data.LiteDbDriver;

public sealed class LiteDatabaseCollection<TData> : IDatabaseCollection<TData>
{
    private readonly ILiteCollectionAsync<TData> _collection;
    public IOperationFactory<TData> Operations { get; } = new OperationFactory<TData>();

    public LiteDatabaseCollection(ILiteCollectionAsync<TData> collection)
        => _collection = collection;

    public IFindQuery<TData, TData> Find(IFilter<TData> filter)
    {
        var query = _collection.Query();
        var expression = GetFilter(filter);

        return expression is null
            ? new LiteQuery<TData>(query)
            : new LiteQuery<TData>(query.Where(expression));
    }

    public async ValueTask<long> CountEntrys(IFilter<TData> filter, CancellationToken token = default)
    {
        var exp = GetFilter(filter);

        return exp is null
            ? await _collection.LongCountAsync()
            : await _collection.LongCountAsync(exp);
    }

    public async ValueTask<DbOperationResult> UpdateOneAsync(IFilter<TData> filter, IUpdate<TData> updater, CancellationToken cancellationToken = default)
    {
        var exp = GetFilter(filter);

        if(exp is null)
            return new DbOperationResult(false, 0, 0);

        var ent = await _collection.FindOneAsync(exp);
        ent = ((LiteUpdate<TData>)updater).Transform(ent);

        await _collection.UpdateAsync(ent);

        return new DbOperationResult(true, 1, 0);
    }

    public async ValueTask InsertOneAsync(TData data, CancellationToken cancellationToken = default)
        => await _collection.UpsertAsync(data);

    public DbOperationResult DeleteOne(IFilter<TData> filter)
    {
        var task = DeleteOneAsync(filter).AsTask();

        return !task.Wait(TimeSpan.FromMinutes(1)) ? new DbOperationResult(false, 0, 0) : task.Result;
    }

    public async ValueTask<DbOperationResult> DeleteOneAsync(IFilter<TData> filter, CancellationToken token = default)
    {
        var exp = GetFilter(filter);

        if(exp is null)
            return new DbOperationResult(false, 0, 0);

        var toDelete = await _collection.FindOneAsync(exp);
        var id = _collection.EntityMapper.Id.Getter(toDelete);

        return await _collection.DeleteAsync(new BsonValue(id)) 
            ? new DbOperationResult(true, 1, 0)
            : new DbOperationResult(true, 0, 0);
    }

    private static BsonExpression? GetFilter(IFilter<TData> filter)
        => ((LiteFilter<TData>)filter).Create();
}