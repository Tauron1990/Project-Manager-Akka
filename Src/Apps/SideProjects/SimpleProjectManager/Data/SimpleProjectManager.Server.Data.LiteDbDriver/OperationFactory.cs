using System.Linq.Expressions;
using System.Reflection;
using SimpleProjectManager.Server.Data.LiteDbDriver.Filter;

namespace SimpleProjectManager.Server.Data.LiteDbDriver;

public sealed class OperationFactory<TData> : IOperationFactory<TData>
{
    public IFilter<TData> Empty { get; } = new EmptyFilter<TData>();

    public IFilter<TData> Eq<TField>(Expression<Func<TData, TField>> selector, TField toEqual)
        => new EqFilter<TData, TField>(selector, toEqual);

    public IFilter<TData> Or(params IFilter<TData>[] filter)
        => new OrFilter<TData>(filter);

    public IUpdate<TData> Set<TField>(Expression<Func<TData, TField>> selector, TField value)
    {
        MemberInfo member = ((MemberExpression)selector.Body).Member;

        return member switch
        {
            PropertyInfo propertyInfo => new LiteUpdate<TData>(
                d =>
                {
                    propertyInfo.SetValue(d, value);

                    return d;
                }),
            FieldInfo fieldInfo => new LiteUpdate<TData>(
                d =>
                {
                    fieldInfo.SetValue(d, value);

                    return d;
                }),
            _ => throw new InvalidOperationException("Only Property or Fiels are Supported")
        };
    }
}