using System.Linq.Expressions;
using Akkatecture.ValueObjects;
using Tauron;

namespace SimpleProjectManager.Server.Data.DataConverters;

public sealed class SingleValueExpressionConverter : BaseExpressionConverter
{
    private readonly Type _toType;
    private readonly Type _fromType;

    public SingleValueExpressionConverter(Type toType, Type fromType)
    {
        _toType = toType;
        _fromType = fromType;
        
        if(fromType.IsAssignableTo(typeof(SingleValueObject<>).MakeGenericType(toType)))
            return;

        throw new InvalidOperationException("The Type is no Vogen ValueType");
    }

    public static bool CanCreate(Type toType, Type fromType)
    {
        try
        {
            return fromType.IsAssignableTo(typeof(SingleValueObject<>).MakeGenericType(toType));
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    public override Expression CreateFromTo(ParameterExpression fromParamameter)
        => Expression.Property(fromParamameter, "Value");

    public override Expression CreateToFrom(ParameterExpression toParameter)
        => Expression.New(_fromType.GetConstructor(new[] { _toType }) ?? throw DefaultError(), toParameter);
}