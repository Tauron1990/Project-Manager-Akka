using LiteDB;
using Tauron;

namespace SimpleProjectManager.Server.Data.LiteDbDriver.Filter;

public sealed class OrFilter<TData> : LiteFilter<TData>
{
    private readonly IFilter<TData>[] _toOr;

    public OrFilter(IFilter<TData>[] toOr)
        => _toOr = toOr;

    protected internal override BsonExpression? Create()
    {
        if(_toOr.Length == 0) return null;
        
        _toOr.Cast<LiteFilter<TData>>().Foreach(f => f.Prepare(IsNot));

        return _toOr.Length == 1 ? ((LiteFilter<TData>)_toOr[0]).Create() : Query.Or(_toOr.Cast<LiteFilter<TData>>().Select(f => f.Create()).ToArray());
    }
}