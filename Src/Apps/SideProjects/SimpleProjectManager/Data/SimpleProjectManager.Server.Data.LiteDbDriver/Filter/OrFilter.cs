namespace SimpleProjectManager.Server.Data.LiteDbDriver.Filter;

public sealed class OrFilter<TData> : LiteFilter<TData>
{
    private readonly IFilter<TData>[] _toOr;

    public OrFilter(IFilter<TData>[] toOr)
        => _toOr = toOr;

    protected internal override bool Run(TData data)
        => _toOr.Cast<LiteFilter<TData>>().Any(filter => filter.Run(data));

    protected internal override IEnumerable<TData> Run(IEnumerable<TData> input)
        => input.Where(data => Run(data));
}