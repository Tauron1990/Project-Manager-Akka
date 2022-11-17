using AutoMapper;

namespace SimpleProjectManager.Server.Data.Data;

public sealed class MappingDatabase<TSource, TDestination> : IDatabaseCollection<TSource>
{
    private readonly IDatabaseCollection<TSource> _databaseCollection;

    public MappingDatabase(IDatabaseCollection<TSource> databaseCollection, IMapper mapper)
    {
        _databaseCollection = databaseCollection;
        Mapper = mapper;
    }

    public IMapper Mapper { get; }

    public IOperationFactory<TSource> Operations => _databaseCollection.Operations;

    public IFindQuery<TSource, TSource> Find(IFilter<TSource> filter)
        => _databaseCollection.Find(filter);

    public ValueTask<long> CountEntrys(IFilter<TSource> filter, CancellationToken token = default)
        => _databaseCollection.CountEntrys(filter, token);

    public ValueTask<DbOperationResult> UpdateOneAsync(IFilter<TSource> filter, IUpdate<TSource> updater, CancellationToken cancellationToken = default)
        => _databaseCollection.UpdateOneAsync(filter, updater, cancellationToken);

    public ValueTask InsertOneAsync(TSource data, CancellationToken cancellationToken = default)
        => _databaseCollection.InsertOneAsync(data, cancellationToken);

    public DbOperationResult DeleteOne(IFilter<TSource> filter)
        => _databaseCollection.DeleteOne(filter);

    public ValueTask<DbOperationResult> DeleteOneAsync(IFilter<TSource> filter, CancellationToken token = default)
        => _databaseCollection.DeleteOneAsync(filter, token);

    public async ValueTask<TDestination[]> ExecuteArray(IFindQuery<TSource, TSource> query, CancellationToken token)
        => await query.ToAsyncEnumerable(token).ProjectTo<TSource, TDestination>(Mapper).ToArrayAsync(token).ConfigureAwait(false);

    public async ValueTask<TRealDestination[]> ExecuteArray<TData, TRealDestination>(IFindQuery<TSource, TData> query, CancellationToken token)
        => await query.ToAsyncEnumerable(token).ProjectTo<TData, TRealDestination>(Mapper).ToArrayAsync(token).ConfigureAwait(false);

    public IAsyncEnumerable<TDestination> ExecuteAsyncEnumerable(IFindQuery<TSource, TSource> query, CancellationToken token)
        => query.ToAsyncEnumerable(token).ProjectTo<TSource, TDestination>(Mapper);

    public IAsyncEnumerable<TRealDestination> ExecuteAsyncEnumerable<TData, TRealDestination>(IFindQuery<TSource, TData> query, CancellationToken token)
        => query.ToAsyncEnumerable(token).ProjectTo<TData, TRealDestination>(Mapper);
    
    public async ValueTask<TDestination?> ExecuteFirstOrDefaultAsync(IFindQuery<TSource, TSource> query, CancellationToken token)
        => await query.ToAsyncEnumerable(token).ProjectTo<TSource, TDestination>(Mapper).FirstOrDefaultAsync(token).ConfigureAwait(false);

    public async ValueTask<TRealDestination?> ExecuteFirstOrDefaultAsync<TData, TRealDestination>(IFindQuery<TSource, TData> query, CancellationToken token)
        => await query.ToAsyncEnumerable(token).ProjectTo<TData, TRealDestination>(Mapper).FirstOrDefaultAsync(token).ConfigureAwait(false);

    public async ValueTask<TDestination> ExecuteFirstAsync(IFindQuery<TSource, TSource> query, CancellationToken token)
        => await query.ToAsyncEnumerable(token).ProjectTo<TSource, TDestination>(Mapper).FirstAsync(token).ConfigureAwait(false);

    public async ValueTask<TRealDestination> ExecuteFirstAsync<TData, TRealDestination>(IFindQuery<TSource, TData> query, CancellationToken token)
        => await query.ToAsyncEnumerable(token).ProjectTo<TData, TRealDestination>(Mapper).FirstAsync(token).ConfigureAwait(false);

    public ValueTask InsertOneAsync(TDestination data, CancellationToken cancellationToken = default)
        => _databaseCollection.InsertOneAsync(Mapper.Map<TSource>(data), cancellationToken);
}