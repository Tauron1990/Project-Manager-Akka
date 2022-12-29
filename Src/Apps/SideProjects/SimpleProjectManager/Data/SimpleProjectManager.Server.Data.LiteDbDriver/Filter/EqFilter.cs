using System.Linq.Expressions;
using FastExpressionCompiler;

namespace SimpleProjectManager.Server.Data.LiteDbDriver.Filter;

public sealed class EqFilter<TData, TField> : LiteFilter<TData>
{
    private readonly EqualityComparer<TField> _equalitiy = EqualityComparer<TField>.Default;
    private readonly Func<TData, TField> _selector;
    private readonly TField _toEqual;

    public EqFilter(Expression<Func<TData, TField>> selector, TField toEqual)
    {
        _selector = selector.CompileFast();
        _toEqual = toEqual;

    }

    protected internal override bool Run(TData data)
    {
        TField fieldValue = _selector(data);

        return IsNot switch
        {
            true when !_equalitiy.Equals(fieldValue, _toEqual) => true,
            false when _equalitiy.Equals(fieldValue, _toEqual) => true,
            _ => false,
        };
    }

    protected internal override IEnumerable<TData> Run(IEnumerable<TData> input)
        => input.Where(Run);
}