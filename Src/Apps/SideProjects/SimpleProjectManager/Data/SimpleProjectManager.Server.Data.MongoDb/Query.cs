using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using MongoDB.Driver;

namespace SimpleProjectManager.Server.Data.MongoDb;

public sealed class Query<TStart, TData> : IFindQuery<TStart, TData>
{
    private readonly IFindFluent<TStart, TData> _findFluent;

    public Query(IFindFluent<TStart, TData> findFluent)
        => _findFluent = findFluent;

    public async ValueTask<TData?> FirstOrDefaultAsync(CancellationToken cancellationToken)
        => await _findFluent.FirstOrDefaultAsync(cancellationToken);

    public TData? FirstOrDefault()
        => _findFluent.FirstOrDefault();

    public IFindQuery<TStart, TResult> Select<TResult>(Expression<Func<TStart, TResult>> transform)
        => new Query<TStart, TResult>(_findFluent.Project(transform));

    public async ValueTask<TData[]> ToArrayAsync(CancellationToken token)
        => (await _findFluent.ToListAsync(token)).ToArray();

    public async ValueTask<TData> FirstAsync(CancellationToken token)
        => await _findFluent.FirstAsync(token);

    public async ValueTask<long> CountAsync(CancellationToken token)
        => await _findFluent.CountDocumentsAsync(token);

    public async ValueTask<TData> SingleAsync(CancellationToken token = default)
        => await _findFluent.SingleAsync(token);

    public async IAsyncEnumerable<TData> ToAsyncEnumerable([EnumeratorCancellation] CancellationToken token = default)
    {
        var cursor = await _findFluent.ToCursorAsync(token);

        while (await cursor.MoveNextAsync(token))
        {
            foreach (TData data in cursor.Current)
                yield return data;
        }
    }
}