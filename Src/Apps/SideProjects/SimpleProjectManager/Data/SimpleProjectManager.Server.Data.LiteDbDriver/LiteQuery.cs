using System.Linq.Expressions;
using FastExpressionCompiler;
using LiteDB;
using SimpleProjectManager.Shared;

namespace SimpleProjectManager.Server.Data.LiteDbDriver;

public sealed class TransformingQuery<TStart, TData> : IFindQuery<TStart, TData>
{
    private readonly LiteQuery<TStart> _original;
    private readonly Func<TStart, TData> _transform;

    public TransformingQuery(LiteQuery<TStart> original, Expression<Func<TStart, TData>> transform)
    {
        _original = original;
        _transform = transform.CompileFast();
    }

    public async ValueTask<TData?> FirstOrDefaultAsync(CancellationToken cancellationToken)
    {
        TStart? result = await _original.FirstOrDefaultAsync(cancellationToken);

        return EqualityComparer<TStart?>.Default.Equals(result, default) ? default : _transform(result!);
    }

    public TData? FirstOrDefault()
    {
        TStart? result = _original.FirstOrDefault();

        return EqualityComparer<TStart?>.Default.Equals(result, default) ? default : _transform(result!);
    }

    public IFindQuery<TStart, TResult> Select<TResult>(Expression<Func<TStart, TResult>> transform)
        => _original.Select(transform);

    public async ValueTask<TData[]> ToArrayAsync(CancellationToken token)
        => (from arg in await _original.ToArrayAsync(token)
            select _transform(arg)
            ).ToArray();

    public async ValueTask<TData> FirstAsync(CancellationToken token)
        => _transform(await _original.FirstAsync(token));

    public ValueTask<long> CountAsync(CancellationToken token)
        => _original.CountAsync(token);

    public async ValueTask<TData> SingleAsync(CancellationToken token = default)
        => _transform(await _original.SingleAsync(token));

    public IAsyncEnumerable<TData> ToAsyncEnumerable(CancellationToken token = default)
        => _original.ToAsyncEnumerable(token).Select(_transform);
}

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