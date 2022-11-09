using System.Linq.Expressions;

namespace SimpleProjectManager.Server.Data;

public interface IFindQuery<TStart, TData>
{
    ValueTask<TData?> FirstOrDefaultAsync(CancellationToken cancellationToken);

    TData? FirstOrDefault();

    IFindQuery<TStart, TResult> Select<TResult>(Expression<Func<TStart, TResult>> transform);
    ValueTask<TData[]> ToArrayAsync(CancellationToken token);
    ValueTask<TData> FirstAsync(CancellationToken token);
    ValueTask<long> CountAsync(CancellationToken token);
    ValueTask<TData> SingleAsync(CancellationToken token = default);

    IAsyncEnumerable<TData> ToAsyncEnumerable(CancellationToken token = default);
}