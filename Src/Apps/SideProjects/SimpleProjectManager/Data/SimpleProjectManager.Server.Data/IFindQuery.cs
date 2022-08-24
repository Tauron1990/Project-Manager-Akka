namespace SimpleProjectManager.Server.Data;

public interface IFindQuery<TData>
{
    ValueTask<TData?> FirstOrDefaultAsync(CancellationToken cancellationToken);
    
    TData? FirstOrDefault();
    IFindQuery<TResult> Project<TResult>(Func<TData, TResult> transform);
    ValueTask<TData[]> ToArrayAsync(CancellationToken token);
}