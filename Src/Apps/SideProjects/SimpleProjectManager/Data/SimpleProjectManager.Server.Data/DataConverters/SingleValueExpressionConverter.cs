using System.Linq.Expressions;
using Akkatecture.ValueObjects;
using OneOf.Types;
using Tauron;

namespace SimpleProjectManager.Server.Data.DataConverters;

public sealed class SingleValueExpressionConverter : BaseExpressionConverter
{
    private readonly Type _toType;
    private readonly Type _fromType;

    private SingleValueExpressionConverter(Type toType, Type fromType)
    {
        _toType = toType;
        _fromType = fromType;
    }

    public static ConverterResult TryCreate(Type toType, Type fromType)
    {
        try
        {
            if(fromType.IsAssignableTo(typeof(SingleValueObject<>).MakeGenericType(toType)))
                return new SingleValueExpressionConverter(toType, fromType);
        }
        catch (ArgumentException)
        {
        }
        
        return default(None);
    }

    public override Expression CreateFromTo(ParameterExpression fromParamameter)
        => Expression.Property(fromParamameter, "Value");

    public override Expression CreateToFrom(ParameterExpression toParameter)
        => Expression.New(_fromType.GetConstructor(new[] { _toType }) ?? throw DefaultError(), toParameter);
}