using System.Linq.Expressions;
using Akkatecture.ValueObjects;

namespace SimpleProjectManager.Server.Data.DataConverters;

internal sealed class SingleValueExpression : IConverterExpression
{
    private readonly Type _fromType;
    private readonly bool _isTo;
    private readonly Type _toType;

    private SingleValueExpression(Type fromType, Type toType, bool isTo)
    {
        _toType = toType;
        _isTo = isTo;
        _fromType = fromType;
    }

    public Expression Generate(Expression from)
        => _isTo
            ? CreateToFrom(from)
            : CreateFromTo(from);

    public static ConverterResult TryCreate(Type fromType, Type toType)
    {
        if(TypeCheck(fromType, toType))
            return ConverterResult.From(new SingleValueExpression(fromType, toType, false));

        return TypeCheck(toType, fromType)
            ? ConverterResult.From(new SingleValueExpression(fromType, toType, true))
            : ConverterResult.None();
    }

    private static bool TypeCheck(Type singleValue, Type targetType)
    {
        try
        {
            return singleValue.IsAssignableTo(typeof(SingleValueObject<>).MakeGenericType(targetType));
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    private Expression CreateFromTo(Expression fromParamameter)
        => Expression.Property(fromParamameter, "Value");

    private Expression CreateToFrom(Expression toParameter)
        => Expression.New(
            _toType.GetConstructor(new[] { _fromType })
         ?? throw new InvalidCastException($"No Contructor for SingleValue Object Found {_toType}"),
            toParameter);
}