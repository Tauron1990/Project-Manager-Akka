using System.Linq.Expressions;

namespace SimpleProjectManager.Server.Data;

public interface IOperationFactory<TData>
{
    IFilter<TData> Empty { get; }

    IFilter<TData> Eq<TField>(Expression<Func<TData, TField>> selector, TField toEqual);

    IUpdate<TData> Set<TField>(Expression<Func<TData, TField>> selector, TField value);
}