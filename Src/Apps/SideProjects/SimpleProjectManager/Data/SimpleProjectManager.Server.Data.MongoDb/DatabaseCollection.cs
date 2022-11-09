using MongoDB.Driver;

namespace SimpleProjectManager.Server.Data.MongoDb;

public class DatabaseCollection<TData> : IDatabaseCollection<TData>
{
    private readonly IMongoCollection<TData> _collection;

    public DatabaseCollection(IMongoCollection<TData> collection)
        => _collection = collection;

    public IOperationFactory<TData> Operations { get; } = new OperationFactory<TData>();

    public IFindQuery<TData, TData> Find(IFilter<TData> filter)
        => new Query<TData, TData>(_collection.Find(GetRealFilter(filter)));

    public async ValueTask<long> CountEntrys(IFilter<TData> filter, CancellationToken token = default)
        => await _collection.CountDocumentsAsync(GetRealFilter(filter), cancellationToken: token);

    public async ValueTask<DbOperationResult> UpdateOneAsync(IFilter<TData> filter, IUpdate<TData> updater, CancellationToken cancellationToken = default)
    {
        UpdateResult? result = await _collection.UpdateOneAsync(GetRealFilter(filter), GetRealUpdater(updater), cancellationToken: cancellationToken);

        return new DbOperationResult(result.IsAcknowledged, result.ModifiedCount, 0);
    }

    public async ValueTask InsertOneAsync(TData data, CancellationToken cancellationToken = default)
        => await _collection.InsertOneAsync(data, cancellationToken: cancellationToken);

    public DbOperationResult DeleteOne(IFilter<TData> filter)
    {
        DeleteResult? result = _collection.DeleteOne(GetRealFilter(filter));

        return new DbOperationResult(result.IsAcknowledged, 0, result.DeletedCount);
    }

    public async ValueTask<DbOperationResult> DeleteOneAsync(IFilter<TData> filter, CancellationToken token = default)
    {
        DeleteResult? result = await _collection.DeleteOneAsync(GetRealFilter(filter), token);

        return new DbOperationResult(result.IsAcknowledged, 0, result.DeletedCount);
    }

    private FilterDefinition<TData> GetRealFilter(IFilter<TData> filter)
        => ((Filter<TData>)filter).FilterDefinition;

    private UpdateDefinition<TData> GetRealUpdater(IUpdate<TData> filter)
        => ((Updater<TData>)filter).UpdateDefinition;
}