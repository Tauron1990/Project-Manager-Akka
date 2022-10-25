using System.Linq.Expressions;
using System.Reflection;

namespace SimpleProjectManager.Server.Data.DataConverters;

internal sealed class ComplexExpression : IConverterExpression
{
    private readonly IFactory _factory;
    private ComplexExpression(IFactory factory)
        => _factory = factory;

    public static ConverterResult TryCreate(Type from, Type to, DataConverter dataConverter)
    {
        var fromPropertys = from.GetProperties();
        var toPropertys = to.GetProperties();

        if(fromPropertys.Any(p => !toPropertys.Any(
                                 tp => string.Equals(tp.Name, p.Name, StringComparison.InvariantCultureIgnoreCase))))
            return ConverterResult.None();

        IFactory? fromTo = CreateFromTo(dataConverter, to, fromPropertys, toPropertys);

        return fromTo is null 
            ? ConverterResult.None() 
            : ConverterResult.From(new ComplexExpression(fromTo));

    }
    
    private interface IFactory
    {
        Expression Create(Expression parameterExpression);
    }

    private static IFactory? CreateFromTo(DataConverter dataConverter, Type to, PropertyInfo[] fromPropertys, PropertyInfo[] toPropertys)
    {
        ConstructorInfo? construcctor = CanConstructor(to, fromPropertys);
        if(construcctor is not null) return new ConstructorFactory(construcctor, fromPropertys, dataConverter);

        var props = CanPropertys(to, fromPropertys);

        return props is not null ? new PropertysFactory(to, props, toPropertys, dataConverter) : null;

    }

    private static ConstructorInfo? CanConstructor(Type target, PropertyInfo[] parameters)
    {
        var constructors = target.GetConstructors();

        ConstructorInfo? constructor = constructors
           .FirstOrDefault(
                constructor => constructor
                   .GetParameters()
                   .Select(p => p.Name?.ToLowerInvariant())
                   .All(
                        parameters
                           .Select(p => p.Name?.ToLowerInvariant())
                           .Contains));

        if(constructor is null) return null;
        var parameterInfos = constructor.GetParameters();

        if(parameterInfos.Length != parameters.Length) return null;

        var propertys = parameters.ToArray();
        
        for (var i = 0; i < parameterInfos.Length; i++)
        {
            ParameterInfo parameter = parameterInfos[i];

            PropertyInfo? propertyInfo = propertys.SingleOrDefault(
                pi => string.Equals(pi.Name, parameter.Name, StringComparison.InvariantCulture));

            if(propertyInfo is null) return null;
            
            parameters[i] = propertyInfo;
        }

        return constructor;
    }

    private static PropertyInfo[]? CanPropertys(Type target, PropertyInfo[] parameters)
    {
        var propertys = target.GetProperties();
        var mappedPropertys = new PropertyInfo[parameters.Length];

        if(propertys.Any(pi => !parameters
                            .Select(p => p.Name.ToLowerInvariant())
                            .Contains(pi.Name.ToLowerInvariant())
                            && !pi.CanWrite))
            return null;

        for (var i = 0; i < parameters.Length; i++)
        {
            PropertyInfo type = parameters[i];
            PropertyInfo? property = propertys.FirstOrDefault(
                pi => string.Equals(pi.Name, type.Name, StringComparison.InvariantCultureIgnoreCase));

            if(property is null) return null;
            
            mappedPropertys[i] = property;
        }

        return mappedPropertys;
    }

    private static Expression CreateConstructor(
        DataConverter dataConverter, ConstructorInfo targetConstructor, 
        Expression input, IEnumerable<PropertyInfo> fromPropertys)
    {
        var parameters = targetConstructor.GetParameters();
        
        var parameterExpressions =
            fromPropertys.Select((pi, i) => WrapConverter(
                                     Expression.Property(input, pi),
                                     new EntryKey(pi.PropertyType, parameters[i].ParameterType), 
                                     dataConverter));

        NewExpression constructor = Expression.New(targetConstructor, parameterExpressions);
        

        return constructor;
    }
    
    private static Expression CreatePropertyAssigment(DataConverter dataConverter,
        Type target, PropertyInfo[] targetPropertys, Expression input, PropertyInfo[] fromPropertys)
    {
        var block = new List<Expression>();
        ParameterExpression targetVariable = Expression.Variable(target);

        block.Add(Expression.Assign(targetVariable, Expression.New(target)));

        for (var i = 0; i < fromPropertys.Length; i++)
        {
            PropertyInfo targetProperty = targetPropertys[i];
            PropertyInfo fromProperty = fromPropertys[i];
            EntryKey key = new(fromProperty.PropertyType, targetProperty.PropertyType);
            
            MemberExpression targetAccessor = Expression.Property(targetVariable, targetProperty);
            Expression parameterAccesor = WrapConverter(Expression.Property(input, fromProperty), key, dataConverter);
            
            block.Add(Expression.Assign(targetVariable, Expression.Assign(targetAccessor, parameterAccesor)));
        }
        
        block.Add(targetVariable);

        return Expression.Block(new[] { targetVariable }, block);
    }

    private static Expression WrapConverter(Expression from, EntryKey key, DataConverter converter)
    {
        IConverterExpression? conv = converter.CreateExcpressionConverter(key);

        return conv is null ? from : conv.Generate(from);
    }
    
    private sealed class ConstructorFactory : IFactory
    {
        private readonly ConstructorInfo _constructor;
        private readonly PropertyInfo[] _from;
        private readonly DataConverter _dataConverter;

        public ConstructorFactory(ConstructorInfo constructor, PropertyInfo[] from, DataConverter dataConverter)
        {
            _constructor = constructor;
            _from = from;
            _dataConverter = dataConverter;
        }

        public Expression Create(Expression parameterExpression)
            => CreateConstructor(_dataConverter, _constructor, parameterExpression, _from);
    }
    
    private sealed class PropertysFactory : IFactory
    {
        private readonly Type _target;
        private readonly PropertyInfo[] _from;
        private readonly PropertyInfo[] _to;
        private readonly DataConverter _dataConverter;

        public PropertysFactory(Type target, PropertyInfo[] from, PropertyInfo[] to, DataConverter dataConverter)
        {
            _target = target;
            _from = from;
            _to = to;
            _dataConverter = dataConverter;
        }

        public Expression Create(Expression parameterExpression)
            => CreatePropertyAssigment(_dataConverter, _target, _to, parameterExpression, _from);
    }

    public Expression Generate(Expression from)
        => _factory.Create(from);
}