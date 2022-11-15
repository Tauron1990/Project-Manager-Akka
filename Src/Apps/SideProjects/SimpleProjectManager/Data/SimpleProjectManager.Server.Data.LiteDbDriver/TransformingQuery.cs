using System.Linq.Expressions;
using FastExpressionCompiler;

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
        TStart? result = await _original.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

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
        => (from arg in await _original.ToArrayAsync(token).ConfigureAwait(false)
            select _transform(arg)
            ).ToArray();

    public async ValueTask<TData> FirstAsync(CancellationToken token)
        => _transform(await _original.FirstAsync(token).ConfigureAwait(false));

    public ValueTask<long> CountAsync(CancellationToken token)
        => _original.CountAsync(token);

    public async ValueTask<TData> SingleAsync(CancellationToken token = default)
        => _transform(await _original.SingleAsync(token).ConfigureAwait(false));

    public IAsyncEnumerable<TData> ToAsyncEnumerable(CancellationToken token = default)
        => _original.ToAsyncEnumerable(token).Select(_transform);
}