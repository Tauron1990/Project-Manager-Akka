namespace SimpleProjectManager.Server.Data.LiteDb;

public sealed class LiteFilter<TData> : IFilter<TData>
{
    private bool _isNot;
    
    public LiteFilter(Func<>)
    {
        
    }
    
    public IFilter<TData> Not { get; }
}