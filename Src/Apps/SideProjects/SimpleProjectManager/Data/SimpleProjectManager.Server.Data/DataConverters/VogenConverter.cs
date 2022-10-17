using System.Linq.Expressions;
using System.Reflection;
using Tauron;
using Vogen;

namespace SimpleProjectManager.Server.Data.DataConverters;

public sealed class VogenConverter<TFrom, TTo> : BaseConverter<TFrom, TTo>
{
    private readonly Type _fromType = typeof(TFrom);
    private readonly Type _toType = typeof(TTo);
    
    public VogenConverter()
    {
        var attr = _fromType.GetCustomAttribute<ValueObjectAttribute>();
        
        if(attr is not null && attr.UnderlyingType == _toType)
            return;

        throw new InvalidOperationException("The Type is no Vogen ValueType");
    }

    public static bool CanCreate()
    {
        var attr = typeof(TFrom).GetCustomAttribute<ValueObjectAttribute>();

        if(attr is null) return false;

        return attr.UnderlyingType == typeof(TTo);
    }

    public override Expression CreateFromTo(ParameterExpression fromParamameter)
        => Expression.Property(fromParamameter, "Value");

    public override Expression CreateToFrom(ParameterExpression toParameter)
    {
        MethodInfo? createMethod = _fromType.GetMethod("From");

        if(createMethod is null)
            throw DefaultError();
        
        
        
        return Expression.Call(createMethod, toParameter);
    }
}