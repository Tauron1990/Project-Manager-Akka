namespace SimpleProjectManager.Server.Data.LiteDbDriver;

public sealed class LiteUpdate<TData> : IUpdate<TData>
{
    public LiteUpdate(Func<TData, TData> transform)
        => Transform = transform;

    public Func<TData, TData> Transform { get; }
}