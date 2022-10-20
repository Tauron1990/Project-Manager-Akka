using System.Linq.Expressions;
using System.Reflection;
using OneOf.Types;
using Tauron;
using Vogen;

namespace SimpleProjectManager.Server.Data.DataConverters;

public sealed class VogenExpressionConverter : BaseExpressionConverter
{
    private readonly Type _fromType;

    private VogenExpressionConverter(Type fromType)
        => _fromType = fromType;

    public static ConverterResult TryCreate(Type toType, Type fromType)
    {
        var attr = fromType.GetCustomAttribute<ValueObjectAttribute>();

        if(attr is not null && attr.UnderlyingType == toType) 
            return new VogenExpressionConverter(fromType);

        return default(None);
    }

    public override Expression CreateFromTo(ParameterExpression fromParamameter)
        => Expression.Property(fromParamameter, "Value");

    public override Expression CreateToFrom(ParameterExpression toParameter)
    {
        MethodInfo? createMethod = _fromType.GetMethod("From");

        if(createMethod is null)
            throw DefaultError();

        var instanceAttributes = new Queue<InstanceAttribute>(_fromType.GetCustomAttributes<InstanceAttribute>().ToArray());
        
        if(instanceAttributes.Count == 0)
            return Expression.Call(createMethod, toParameter);

        
        return Expression.Block(new[] { toParameter }, CreateToFrom(toParameter, createMethod, instanceAttributes));
    }

    private Expression CreateToFrom(ParameterExpression toParameter, MethodInfo creation, Queue<InstanceAttribute> attributes)
    {
        if(attributes.Count == 0)
            return Expression.Call(creation, toParameter);

        var instanceAttribute = attributes.Dequeue();
        
        MemberExpression valueAcess = Expression.Field(null, _fromType.GetField(instanceAttribute.Name) ?? throw DefaultError());
        BinaryExpression condition =
            Expression.Equal(
                Expression.Property(valueAcess, "Value"),
                toParameter);

        return Expression.Condition(condition, valueAcess, CreateToFrom(toParameter, creation, attributes));
    }
}