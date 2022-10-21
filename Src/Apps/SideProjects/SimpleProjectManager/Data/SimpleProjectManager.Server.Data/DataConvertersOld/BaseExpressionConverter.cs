using System.Linq.Expressions;
using System.Reflection;
using SimpleProjectManager.Server.Data.DataConverters;

namespace SimpleProjectManager.Server.Data.DataConvertersOld;

internal abstract class BaseExpressionConverter
{
    public abstract Expression CreateFromTo(Expression fromParamameter);

    public abstract Expression CreateToFrom(Expression toParameter);

    protected Exception DefaultError()
        => new InvalidOperationException("No Factory Expression coud Created");
    
    protected static ConstructorInfo? CanConstructor(Type target, PropertyInfo[] parameters)
    {
        var constructors = target.GetConstructors();

        ConstructorInfo? constructor = constructors
           .FirstOrDefault(constructor => constructor.GetParameters().Select(p => p.ParameterType).All(parameters.Select(p => p.PropertyType).Contains));

        if(constructor is null) return null;
        var parameterInfos = constructor.GetParameters();

        if(parameterInfos.Length != parameters.Length) return null;

        var propertys = parameters.ToArray();
        
        for (var i = 0; i < parameterInfos.Length; i++)
        {
            ParameterInfo parameter = parameterInfos[i];

            PropertyInfo? propertyInfo = propertys.SingleOrDefault(
                pi => pi.PropertyType == parameter.ParameterType
                   && pi.Name.ToLowerInvariant() == parameter.Name?.ToLowerInvariant());

            if(propertyInfo is null) return null;
            
            parameters[i] = propertyInfo;
        }

        return constructor;
    }

    protected static PropertyInfo[]? CanPropertys(Type target, PropertyInfo[] parameters)
    {
        var propertys = target.GetProperties();
        var mappedPropertys = new PropertyInfo[parameters.Length];

        if(propertys.Any(pi => !parameters.Select(p => p.PropertyType).Contains(pi.PropertyType)))
            return null;

        for (var i = 0; i < parameters.Length; i++)
        {
            PropertyInfo type = parameters[i];
            PropertyInfo? property = propertys.FirstOrDefault(
                pi => pi.PropertyType == type 
                && pi.Name.ToLowerInvariant() == type.Name.ToLowerInvariant());

            if(property is null) return null;
            
            mappedPropertys[i] = property;
        }

        return mappedPropertys;
    }

    protected static Expression CreateConstructor(
        DataConverter dataConverter, ConstructorInfo targetConstructor, 
        ParameterExpression input, IEnumerable<PropertyInfo> parameters)
    {
        var parameterExpressions =
            parameters.Select(pi => WrapConverter(Expression.Property(input, pi), entryKey, dataConverter));

        NewExpression constructor = Expression.New(targetConstructor, parameterExpressions);
        

        return constructor;
    }
    
    protected static Expression CreatePropertyAssigment(DataConverter dataConverter,
        Type target, PropertyInfo[] targetPropertys, ParameterExpression input, PropertyInfo[] parameters)
    {
        var block = new List<Expression>();
        ParameterExpression targetVariable = Expression.Variable(target);

        block.Add(Expression.Assign(targetVariable, Expression.New(target)));

        for (var i = 0; i < parameters.Length; i++)
        {
            MemberExpression targetAccessor = Expression.Property(targetVariable, targetPropertys[i]);
            Expression parameterAccesor = WrapConverter(Expression.Property(input, parameters[i]), key, dataConverter);
            
            block.Add(Expression.Assign(targetVariable, Expression.Assign(targetAccessor, parameterAccesor)));
        }
        
        block.Add(targetVariable);

        return Expression.Block(new[] { input, targetVariable }, block);
    }

    private static Expression WrapConverter(Expression from, EntryKey key, DataConverter converter)
    {
        BaseExpressionConverter? conv = converter.CreateExcpressionConverter(key);

        if(conv is null) return from;
        if(key.From == from.Type)
            return conv.CreateFromTo(from);
        return key.To == from.Type ? conv.CreateToFrom(from) : from;

    }
    
    /*var fromProps = _fromType.GetProperties();

    ConstructorInfo? constructor = CanConstructor(_toType, fromProps);

        if(constructor is not null)
    return CreateConstructor(constructor, fromParamameter, fromProps);

    var toProps = _toType.GetProperties();

    var targetPropertys = CanPropertys(_toType, toProps);

        if(targetPropertys is not null)
    return CreatePropertyAssigment(_toType, targetPropertys, fromParamameter, fromProps);

        throw DefaultError();*/
}