using System.Linq.Expressions;
using System.Reflection;
using OneOf.Types;

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

        if(fromPropertys.Any(p => !toPropertys.Select(tp => tp.PropertyType).Contains(p.PropertyType)))
            return default(None);
        
        
    }

    public override Expression CreateFromTo(ParameterExpression fromParamameter)
        => throw new NotImplementedException();

    public override Expression CreateToFrom(ParameterExpression toParameter)
        => throw new NotImplementedException();
    
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

        public PropertysFactory(Type target, PropertyInfo[] from, PropertyInfo[] to)
        {
            _target = target;
            _from = from;
            _to = to;
        }

        public Expression Create(ParameterExpression parameterExpression)
            => CreatePropertyAssigment(_target, _to, parameterExpression, _from);
    }
}