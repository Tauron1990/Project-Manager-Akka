namespace SimpleProjectManager.Server.Data;

public interface IFilter<TData>
{
    IFilter<TData> Not { get; }
}