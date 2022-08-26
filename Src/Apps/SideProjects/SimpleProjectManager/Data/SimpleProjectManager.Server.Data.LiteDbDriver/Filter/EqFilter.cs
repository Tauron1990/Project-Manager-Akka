using System.Linq.Expressions;
using System.Reflection;
using Akkatecture.Extensions;
using LiteDB;
using LiteDB.Async;

namespace SimpleProjectManager.Server.Data.LiteDbDriver.Filter;

public sealed class EqFilter<TData, TField> : LiteFilter<TData>
{
    private readonly Expression<Func<TData, TField>> _selector;
    private readonly TField _toEqual;

    public EqFilter(Expression<Func<TData, TField>> selector, TField toEqual)
    {
        _selector = selector;
        _toEqual = toEqual;

    }
    protected internal override BsonExpression? Create()
    {
        var name = string.Empty;

        if(_selector.Body is not MemberExpression memberExpression)
            throw new InvalidOperationException("Selector is no Member Expression");

        switch (memberExpression.Member)
        {
            case PropertyInfo:
            case FieldInfo:
                name = memberExpression.Member.Name;
                break;
            default:
                throw new InvalidOperationException("Member Selector must be Property or Field");
        }

        var value = BsonMapper.Global.Serialize(_toEqual);
        
        return IsNot 
            ? Query.Not(name, value)
            : Query.EQ(name, value);
    }
}