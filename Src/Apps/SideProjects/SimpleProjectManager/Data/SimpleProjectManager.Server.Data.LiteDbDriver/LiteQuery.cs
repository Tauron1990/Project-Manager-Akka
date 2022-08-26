using System.Linq.Expressions;
using LiteDB.Async;

namespace SimpleProjectManager.Server.Data.LiteDbDriver;

public sealed class LiteFullQuery<TStart> : LiteQuery<TStart, TStart>
{
    private readonly ILiteQueryableAsync<TStart> _query;
    public LiteFullQuery(ILiteQueryableAsync<TStart> query) : base(query)
        => _query = query;

    public override IFindQuery<TStart, TResult> Project<TResult>(Expression<Func<TStart, TResult>> transform)
        => new LiteQuery<TStart, TResult>(_query.Select(transform));
}


public class LiteQuery<TStart, TData> : IFindQuery<TStart, TData>
{
    private readonly ILiteQueryableAsyncResult<TData> _query;

    public LiteQuery(ILiteQueryableAsyncResult<TData> query)
        => _query = query;

    public async ValueTask<TData?> FirstOrDefaultAsync(CancellationToken cancellationToken)
        => await _query.FirstOrDefaultAsync();

    public TData? FirstOrDefault()
        => _query.FirstOrDefaultAsync().Result;

    public virtual IFindQuery<TStart, TResult> Project<TResult>(Expression<Func<TStart, TResult>> transform)
        => throw new NotSupportedException("Projection is on Result Query not Supported");

    public async ValueTask<TData[]> ToArrayAsync(CancellationToken token)
        => await _query.ToArrayAsync();

    public async ValueTask<TData> FirstAsync(CancellationToken token)
        => await _query.FirstAsync() ;

    public async ValueTask<long> CountAsync(CancellationToken token)
        => await _query.CountAsync();

    public async ValueTask<TData> SingleAsync(CancellationToken token = default)
        => await _query.SingleAsync();
}