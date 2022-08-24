namespace SimpleProjectManager.Server.Data;

public interface IDatabaseCollection<TData>
{
    IOperationFactory<TData> Operations { get; }
    
    IFindQuery<TData> Find(IFilter<TData> filter);

    Task<long> CountEntrys(IFilter<TData> filter, CancellationToken token);
    
    ValueTask<DbOperationResult> UpdateOneAsync(IFilter<TData> filter, IUpdate<TData> updater, CancellationToken cancellationToken);
    ValueTask InsertOneAsync(TData data, CancellationToken cancellationToken);
    DbOperationResult DeleteOne(IFilter<TData> filter);
}