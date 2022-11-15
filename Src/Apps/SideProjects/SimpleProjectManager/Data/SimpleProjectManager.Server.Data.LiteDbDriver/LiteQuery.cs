using System.Linq.Expressions;
using LiteDB;
using SimpleProjectManager.Shared;

namespace SimpleProjectManager.Server.Data.LiteDbDriver;

public sealed class LiteQuery<TStart> : IFindQuery<TStart, TStart>
{
    private readonly Func<IEnumerable<TStart>, IEnumerable<TStart>> _filter;
    private readonly ILiteQueryableResult<TStart> _query;

    public LiteQuery(ILiteQueryableResult<TStart> query, Func<IEnumerable<TStart>, IEnumerable<TStart>> filter)
    {
        _query = query;
        _filter = filter;
    }

    public ValueTask<TStart?> FirstOrDefaultAsync(CancellationToken cancellationToken)
        => To.VTask(GetData().FirstOrDefault);

    public TStart? FirstOrDefault()
        => GetData().FirstOrDefault();

    public IFindQuery<TStart, TResult> Select<TResult>(Expression<Func<TStart, TResult>> transform)
        => new TransformingQuery<TStart, TResult>(this, transform);

    public ValueTask<TStart[]> ToArrayAsync(CancellationToken token)
        => To.VTask(GetData().ToArray);

    public ValueTask<TStart> FirstAsync(CancellationToken token)
        => To.VTask(GetData().First);

    public ValueTask<long> CountAsync(CancellationToken token)
        => To.VTask(() => (long)GetData().Count());

    public ValueTask<TStart> SingleAsync(CancellationToken token = default)
        => To.VTask(GetData().Single);

    public IAsyncEnumerable<TStart> ToAsyncEnumerable(CancellationToken token = default)
        => GetData().ToAsyncEnumerable();

    private IEnumerable<TStart> GetData()
        => _filter(_query.ToEnumerable());
}