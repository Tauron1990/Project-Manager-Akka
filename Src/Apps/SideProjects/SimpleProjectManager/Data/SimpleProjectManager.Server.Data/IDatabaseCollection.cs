namespace SimpleProjectManager.Server.Data;

public interface IDatabaseCollection<TData>
{
    IOperationFactory<TData> Operations { get; }

    IFindQuery<TData, TData> Find(IFilter<TData> filter);

    ValueTask<long> CountEntrys(IFilter<TData> filter, CancellationToken token = default);

    ValueTask<DbOperationResult> UpdateOneAsync(IFilter<TData> filter, IUpdate<TData> updater, CancellationToken cancellationToken = default);
    ValueTask InsertOneAsync(TData data, CancellationToken cancellationToken = default);
    DbOperationResult DeleteOne(IFilter<TData> filter);
    ValueTask<DbOperationResult> DeleteOneAsync(IFilter<TData> filter, CancellationToken token = default);
}