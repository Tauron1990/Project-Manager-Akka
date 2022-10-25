using System.Linq.Expressions;
using System.Reflection;
using Tauron;
using Vogen;

namespace SimpleProjectManager.Server.Data.DataConverters;

internal sealed class VogenExpression : IConverterExpression
{
    private readonly Type _from;
    private readonly Type _to;
    private readonly bool _fromTo;

    private VogenExpression(Type from, Type to, bool fromTo)
    {
        _from = from;
        _to = to;
        _fromTo = fromTo;
    }

    public static ConverterResult TryCreate(Type from, Type to)
    {
        if(Validate(from, to))
            return ConverterResult.From(new VogenExpression(from, to, false));
        return Validate(to, from) ? ConverterResult.From(new VogenExpression(from, to, true)) : ConverterResult.None();

    }

    private static bool Validate(ICustomAttributeProvider fromType, Type toType)
    {
        var attr = fromType.GetCustomAttribute<ValueObjectAttribute>();

        return attr.HasValue && attr.Value.UnderlyingType == toType;
    }

    public Expression Generate(Expression from)
        => !_fromTo ? CreateFromTo(from) : CreateToFrom(from);

    private static Expression CreateFromTo(Expression fromParamameter)
        => Expression.Property(fromParamameter, "Value");

    private Expression CreateToFrom(Expression toParameter)
    {
        MethodInfo? createMethod = _to.GetMethod("From");

        if(createMethod is null)
            throw new InvalidOperationException("Vogen From Method not Found");

        var instanceAttributes = new Queue<InstanceAttribute>(_to.GetCustomAttributes<InstanceAttribute>().ToArray());
        
        return instanceAttributes.Count == 0 ? Expression.Call(createMethod, toParameter) : CreateToFrom(toParameter, createMethod, instanceAttributes);
    }

    private Expression CreateToFrom(Expression toParameter, MethodInfo creation, Queue<InstanceAttribute> attributes)
    {
        if(attributes.Count == 0)
            return Expression.Call(creation, toParameter);

        InstanceAttribute instanceAttribute = attributes.Dequeue();
        
        MemberExpression valueAcess = Expression.Field(null, _to.GetField(instanceAttribute.Name) 
                                                          ?? throw new InvalidOperationException("No Vogen Instance Field Found"));
        BinaryExpression condition =
            Expression.Equal(
                Expression.Property(valueAcess, "Value"),
                toParameter);

        return Expression.Condition(condition, valueAcess, CreateToFrom(toParameter, creation, attributes));
    }
}