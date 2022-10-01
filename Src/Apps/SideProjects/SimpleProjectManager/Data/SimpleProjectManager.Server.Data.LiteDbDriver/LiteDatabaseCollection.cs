using LiteDB;
using SimpleProjectManager.Shared;

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
        var expression = RunFilter(filter);

        return new LiteQuery<TData>(query, expression);
    }

    public ValueTask<long> CountEntrys(IFilter<TData> filter, CancellationToken token = default)
    {
        var exp = RunFilter(filter);

        return To.VTask(() => exp(_collection.FindAll()).LongCount());
    }

    public ValueTask<DbOperationResult> UpdateOneAsync(IFilter<TData> filter, IUpdate<TData> updater, CancellationToken cancellationToken = default)
        => To.VTask(
            () =>
            {
                var exp = RunFilter(filter);

                var ent = exp(_collection.FindAll()).Single();
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
                var exp = RunFilter(filter);

                var toDelete = exp(_collection.FindAll()).Single();
                var id = _collection.EntityMapper.Id.Getter(toDelete);

                return _collection.Delete(new BsonValue(id))
                    ? new DbOperationResult(true, 1, 0)
                    : new DbOperationResult(true, 0, 0);
            });

    private static Func<IEnumerable<TData>, IEnumerable<TData>> RunFilter(IFilter<TData> filter)
        => input => ((LiteFilter<TData>)filter).Run(input);
}