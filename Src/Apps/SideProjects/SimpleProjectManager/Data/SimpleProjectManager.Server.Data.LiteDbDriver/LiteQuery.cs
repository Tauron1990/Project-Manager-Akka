using System.Linq.Expressions;
using FastExpressionCompiler;
using LiteDB;

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

    public async ValueTask<TData> FirstOrDefaultAsync(CancellationToken cancellationToken)
    {
        var result = await _original.FirstOrDefaultAsync(cancellationToken);

        return EqualityComparer<TStart?>.Default.Equals(result, default) ? default : _transform(result!);
    }

    public TData? FirstOrDefault()
    {
        var result = _original.FirstOrDefault();

        return EqualityComparer<TStart?>.Default.Equals(result, default) ? default : _transform(result!);
    }

    public IFindQuery<TStart, TResult> Project<TResult>(Expression<Func<TStart, TResult>> transform)
        => _original.Project(transform);

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
}

public sealed class LiteQuery<TStart> : IFindQuery<TStart, TStart>
{
    private readonly ILiteQueryableResult<TStart> _query;

    public LiteQuery(ILiteQueryableResult<TStart> query)
        => _query = query;

    public ValueTask<TStart> FirstOrDefaultAsync(CancellationToken cancellationToken)
        => To.Task(_query.FirstOrDefault);

    public TStart FirstOrDefault()
        => _query.FirstOrDefault();

    public IFindQuery<TStart, TResult> Project<TResult>(Expression<Func<TStart, TResult>> transform)
        => new TransformingQuery<TStart, TResult>(this, transform);

    public ValueTask<TStart[]> ToArrayAsync(CancellationToken token)
        => To.Task(_query.ToArray);

    public ValueTask<TStart> FirstAsync(CancellationToken token)
        => To.Task(_query.First);

    public ValueTask<long> CountAsync(CancellationToken token)
        => To.Task(() => (long)_query.Count());

    public ValueTask<TStart> SingleAsync(CancellationToken token = default)
        => To.Task(_query.Single);
}