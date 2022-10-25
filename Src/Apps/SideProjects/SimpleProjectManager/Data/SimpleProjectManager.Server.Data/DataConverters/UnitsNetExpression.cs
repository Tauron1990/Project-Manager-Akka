using System.Linq.Expressions;
using System.Reflection;
using Tauron;
using UnitsNet;

namespace SimpleProjectManager.Server.Data.DataConverters;

internal sealed class UnitsNetExpression : IConverterExpression
{
    private readonly Type _from;
    private readonly Type _to;
    private readonly bool _isData;

    private UnitsNetExpression(Type from, Type to, bool isData)
    {
        _from = from;
        _to = to;
        this._isData = isData;

    }

    public static ConverterResult TryCreate(Type from, Type to)
    {
        if(from.IsAssignableTo(typeof(IQuantity)) && to == typeof(UnitNetData))
            return ConverterResult.From(new UnitsNetExpression(from, to, false));
        return to.IsAssignableTo(typeof(IQuantity)) && from == typeof(UnitNetData) 
            ? ConverterResult.From(new UnitsNetExpression(from, to, true)) 
            : ConverterResult.None();
    }

    public Expression Generate(Expression from)
        => _isData
            ? CreateData(from)
            : CreateQuantity(from);

    private Expression CreateQuantity(Expression fromParamameter)
    {
        ParameterExpression variable = Expression.Variable(typeof(UnitNetData));

        MethodInfo asMethod = typeof(IQuantity).GetMethod("As", new[] { typeof(UnitSystem) })
                           ?? throw new InvalidOperationException("Quantity \"As\" not Found");
        
        Expression[] expressions =
        {
            Expression.Assign(variable, Expression.New(typeof(UnitNetData))),
            Expression.Assign(
                Expression.Property(variable, nameof(UnitNetData.Type)),
                Expression.Constant(fromParamameter.Type.AssemblyQualifiedName)),
            Expression.Assign(
                Expression.Property(variable, nameof(UnitNetData.Value)),
                Expression.Call(
                    Expression.Convert(fromParamameter, typeof(IQuantity)),
                    asMethod,
                    SiSystem())),
            variable
        };

        return Expression.Block(new[] { variable }, expressions);
    }

    private Expression SiSystem()
        => Expression.Property(null, typeof(UnitSystem).GetProperty(nameof(UnitSystem.SI)) 
                                  ?? throw new InvalidCastException("UnitSystem SI not Found"));
    
    private Expression CreateData(Expression toParameter)
    {
        MethodCallExpression typeGetter = Expression.Call(
            null,
            typeof(Type).GetMethod(nameof(Type.GetType), new []{typeof(string)}) 
         ?? throw new InvalidCastException("\"GetType\" Method not Found"),
            Expression.Property(toParameter, nameof(UnitNetData.Type)));

        MethodCallExpression objectFactory = Expression.Call(
            Expression.Constant(FastReflection.Shared),
            typeof(FastReflection).GetMethod(nameof(FastReflection.GetCreator), new[] { typeof(Type), typeof(Type[]) }) 
         ?? throw new InvalidOperationException("Fast Refelction GetCreator not Found"),
            typeGetter,
            Expression.Constant(new[] { typeof(double), typeof(UnitSystem) }));

        MethodCallExpression targetObject = Expression.Call(
            objectFactory,
            typeof(Func<object[], object>).GetMethod("Invoke") 
         ?? throw new InvalidOperationException("Delegate Invoke not Found"),
            Expression.NewArrayInit(
                typeof(object),
                Expression.Convert(
                    Expression.Property(toParameter, nameof(UnitNetData.Value)),
                    typeof(object)),
                Expression.Convert(
                    SiSystem(),
                    typeof(object))));

        return Expression.Convert(targetObject, _to);
    }
}