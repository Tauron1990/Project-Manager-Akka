namespace SimpleProjectManager.Server.Data.LiteDbDriver.Filter;

public sealed class EmptyFilter<TData> : LiteFilter<TData>
{
    protected internal override bool Run(TData data)
        => true;

    protected internal override IEnumerable<TData> Run(IEnumerable<TData> input)
        => input;
}