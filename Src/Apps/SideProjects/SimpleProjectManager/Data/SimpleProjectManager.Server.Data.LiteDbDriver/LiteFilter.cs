using LiteDB;

namespace SimpleProjectManager.Server.Data.LiteDbDriver;

public abstract class LiteFilter<TData> : IFilter<TData>
{
    protected bool IsNot { get; set; }

    protected internal void Prepare(bool isNot)
        => IsNot = isNot;
    
    protected internal abstract BsonExpression? Create();

    public IFilter<TData> Not
    {
        get
        {
            IsNot = true;

            return this;
        }
    }
}