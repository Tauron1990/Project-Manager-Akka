using System.Linq.Expressions;
using System.Reflection;
using OneOf.Types;
using SimpleProjectManager.Server.Data.DataConvertersOld;

namespace SimpleProjectManager.Server.Data.DataConverters;



public sealed class ComplexExpressionConverter : BaseExpressionConverter
{
    private readonly IFactory _fromTo;
    private readonly IFactory _toFrom;

    private ComplexExpressionConverter(IFactory fromTo, IFactory toFrom)
    {
        _fromTo = fromTo;
        _toFrom = toFrom;
    }
    
    public static ConverterResult TryCreate(Type from, Type to)
    {
        var fromPropertys = from.GetProperties();
        var toPropertys = to.GetProperties();

        if(fromPropertys.Any(p => !toPropertys.Any(
                                 tp => tp.PropertyType == p.PropertyType 
                                    && string.Equals(tp.Name, p.Name, StringComparison.InvariantCultureIgnoreCase))))
            return default(None);

        IFactory? fromTo = CreateFromTo(from, to, fromPropertys, toPropertys);
        IFactory? toFrom = CreateToFrom(from, to, fromPropertys, toPropertys);

        if(fromTo is null || toFrom is null) return default(None);

        return new ComplexExpressionConverter(fromTo, toFrom);
    }

    private static IFactory? CreateFromTo(Type from, Type to, PropertyInfo[] fromPropertys, PropertyInfo[] toPropertys)
    {
        ConstructorInfo? construcctor = CanConstructor(to, fromPropertys);
        if(construcctor is not null) return new ConstructorFactory(construcctor, fromPropertys);

        var props = CanPropertys(to, fromPropertys);

        return props is not null ? new PropertysFactory(to, props, toPropertys) : null;

    }

    private static IFactory? CreateToFrom(Type from, Type to, PropertyInfo[] fromPropertys, PropertyInfo[] toPropertys)
    {
        ConstructorInfo? constructor = CanConstructor(from, toPropertys);

        if(constructor is not null) return new ConstructorFactory(constructor, toPropertys);

        var props = CanPropertys(from, toPropertys);

        return props is not null ? new PropertysFactory(from, props, fromPropertys) : null;
    }
    
    public override Expression CreateFromTo(Expression fromParamameter)
        => _fromTo.Create(fromParamameter);

    public override Expression CreateToFrom(Expression toParameter)
        => _toFrom.Create(toParameter);
    
    private interface IFactory
    {
        Expression Create(ParameterExpression parameterExpression);
    }
    
    private sealed class ConstructorFactory : IFactory
    {
        private readonly ConstructorInfo _constructor;
        private readonly PropertyInfo[] _from;

        public ConstructorFactory(ConstructorInfo constructor, PropertyInfo[] from)
        {
            _constructor = constructor;
            _from = from;
        }

        public Expression Create(ParameterExpression parameterExpression)
            => CreateConstructor(_constructor, parameterExpression, _from);
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

        public Expression Create(ParameterExpression parameterExpression)
            => CreatePropertyAssigment(_target, _to, parameterExpression, _from);
    }
}