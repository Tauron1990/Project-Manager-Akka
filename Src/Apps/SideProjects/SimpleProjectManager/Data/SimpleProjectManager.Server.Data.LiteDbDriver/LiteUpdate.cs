using LiteDB;
using LiteDB.Async;

namespace SimpleProjectManager.Server.Data.LiteDbDriver;

public sealed class LiteUpdate<TData> : IUpdate<TData>
{
    public Func<TData, TData> Transform { get; }

    public LiteUpdate(Func<TData, TData> transform)
        => Transform = transform;
}