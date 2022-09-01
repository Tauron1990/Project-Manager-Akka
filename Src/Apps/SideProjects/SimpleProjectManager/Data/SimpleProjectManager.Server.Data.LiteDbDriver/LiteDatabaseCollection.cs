using LiteDB;

namespace SimpleProjectManager.Server.Data.LiteDbDriver;

public sealed class LiteDatabaseCollection<TData> : IDatabaseCollection<TData>
{
    private readonly ILiteCollection<TData> _collection;
    public IOperationFactory<TData> Operations { get; } = new OperationFactory<TData>();

    public LiteDatabaseCollection(ILiteCollection<TData> collection)
        => _collection = collection;

    public IFindQuery<TData, TData> Find(IFilter<TData> filter)
    {
        var query = _collection.Query();
        var expression = GetFilter(filter);

        return expression is null
            ? new LiteQuery<TData>(query)
            : new LiteQuery<TData>(query.Where(expression));
    }

    public ValueTask<long> CountEntrys(IFilter<TData> filter, CancellationToken token = default)
    {
        var exp = GetFilter(filter);

        return exp is null
            ? To.VTask(_collection.LongCount)
            : To.VTask(() => _collection.LongCount(exp));
    }

    public ValueTask<DbOperationResult> UpdateOneAsync(IFilter<TData> filter, IUpdate<TData> updater, CancellationToken cancellationToken = default)
        => To.VTask(
            () =>
            {
                var exp = GetFilter(filter);

                if(exp is null)
                    return new DbOperationResult(false, 0, 0);

                var ent = _collection.FindOne(exp);
                ent = ((LiteUpdate<TData>)updater).Transform(ent);

                _collection.Update(ent);

                return new DbOperationResult(true, 1, 0);
            });

    public ValueTask InsertOneAsync(TData data, CancellationToken cancellationToken = default)
        => To.VTaskV(() => _collection.Upsert(data));

    public DbOperationResult DeleteOne(IFilter<TData> filter)
    {
        var task = DeleteOneAsync(filter).AsTask();

        return !task.Wait(TimeSpan.FromMinutes(1)) ? new DbOperationResult(false, 0, 0) : task.Result;
    }

    public ValueTask<DbOperationResult> DeleteOneAsync(IFilter<TData> filter, CancellationToken token = default)
        => To.VTask(
            () =>
            {
                var exp = GetFilter(filter);

                if(exp is null)
                    return new DbOperationResult(false, 0, 0);

                var toDelete = _collection.FindOne(exp);
                var id = _collection.EntityMapper.Id.Getter(toDelete);

                return _collection.Delete(new BsonValue(id))
                    ? new DbOperationResult(true, 1, 0)
                    : new DbOperationResult(true, 0, 0);
            });

    private static BsonExpression? GetFilter(IFilter<TData> filter)
        => ((LiteFilter<TData>)filter).Create();
}