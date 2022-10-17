using System.Linq.Expressions;
using System.Reflection;

namespace SimpleProjectManager.Server.Data.DataConverters;

public abstract class BaseConverter<TFrom, TTo>
{
    public abstract Expression CreateFromTo(ParameterExpression fromParamameter);

    public abstract Expression CreateToFrom(ParameterExpression toParameter);

    protected Exception DefaultError()
        => new InvalidOperationException("No Factory Expression coud Created");
    
    /*protected ConstructorInfo? CanConstructor(Type target, PropertyInfo[] parameters)
    {
        var constructors = target.GetConstructors();

        ConstructorInfo? constructor = constructors
           .FirstOrDefault(constructor => constructor.GetParameters().Select(p => p.ParameterType).All(parameters.Contains));

        if(constructor is null) return null;
        var parameterTypes = constructor.GetParameters();

        if(parameterTypes.Length != parameters.Length) return null;
        
        for (var i = 0; i < parameterTypes.Length; i++)
            parameters[i] = parameterTypes[i].ParameterType;

        return constructor;
    }

    protected PropertyInfo[]? CanPropertys(Type target, PropertyInfo[] parameters)
    {
        var propertys = target.GetProperties();
        var mappedPropertys = new PropertyInfo[parameters.Length];

        if(propertys.Any(pi => !parameters.Contains(pi.PropertyType)))
            return null;

        for (var i = 0; i < parameters.Length; i++)
        {
            Type type = parameters[i];
            PropertyInfo property = propertys.First(pi => pi.PropertyType == type);
            mappedPropertys[i] = property;
        }

        return mappedPropertys;
    }

    protected Expression CreateConstructor(ConstructorInfo targetConstructor, ParameterExpression input, PropertyInfo[] parameters)
    {
        var block = new List<Expression>();
        LabelTarget returnlabel = Expression.Label();

        IEnumerable<Expression> parameterExpressions =
            parameters.Select(pi => Expression.Property(input, pi));

        NewExpression constructor = Expression.New(targetConstructor, parameterExpressions);
        
        block.Add(Expression.Return(returnlabel, constructor));

        return Expression.Block(new[] { input }, block);
    }
    
    protected Expression CreatePropertyAssigment(Type target, PropertyInfo[] targetPropertys, ParameterExpression input, PropertyInfo[] parameters)
    {
        var block = new List<Expression>();
        LabelTarget returnlabel = Expression.Label();
        ParameterExpression targetVariable = Expression.Variable(target);

        block.Add(Expression.Assign(targetVariable, Expression.New(target)));

        for (var i = 0; i < parameters.Length; i++)
        {
            MemberExpression targetAccessor = Expression.Property(targetVariable, targetPropertys[i]);
            MemberExpression parameterAccesor = Expression.Property(input, parameters[i]);
            
            block.Add(Expression.Assign(targetVariable, Expression.Assign(targetAccessor, parameterAccesor)));
        }
        
        block.Add(Expression.Return(returnlabel, targetVariable));

        return Expression.Block(new[] { input, targetVariable }, block);
    }*/
    
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