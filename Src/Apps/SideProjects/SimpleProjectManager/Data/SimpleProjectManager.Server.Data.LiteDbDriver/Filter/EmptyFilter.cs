using LiteDB;

namespace SimpleProjectManager.Server.Data.LiteDbDriver.Filter;

public sealed class EmptyFilter<TData> : LiteFilter<TData>
{
    protected internal override BsonExpression? Create()
        => null;
}