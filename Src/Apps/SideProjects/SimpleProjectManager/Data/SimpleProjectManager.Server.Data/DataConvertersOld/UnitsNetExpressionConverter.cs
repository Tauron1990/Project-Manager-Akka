using System.Linq.Expressions;
using System.Reflection;
using SimpleProjectManager.Server.Data.DataConvertersOld;
using Tauron;
using UnitsNet;

namespace SimpleProjectManager.Server.Data.DataConverters;

public sealed class UnitsNetExpressionConverter : BaseExpressionConverter
{
    private readonly Type _from;

    public UnitsNetExpressionConverter(Type from)
    {
        _from = from;

        if(from.IsAssignableTo(typeof(IQuantity)))
            return;

        throw new InvalidOperationException("The type must Implement IQuantity");
    }

    public static bool CanCreate(Type from)
        => from.IsAssignableTo(typeof(IQuantity));

    public override Expression CreateFromTo(Expression fromParamameter)
    {
        LabelTarget returnlabel = Expression.Label();
        ParameterExpression variable = Expression.Variable(typeof(UnitNetData));

        MethodInfo asMethod = typeof(IQuantity).GetMethod("As", new[] { typeof(UnitSystem) }) ?? throw DefaultError();
        
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

        return Expression.Block(new[] { fromParamameter, variable }, expressions);
    }

    private Expression SiSystem()
        => Expression.Property(null, typeof(UnitSystem).GetProperty(nameof(UnitSystem.SI)) ?? throw DefaultError());
    
    public override Expression CreateToFrom(Expression toParameter)
    {
        MethodCallExpression typeGetter = Expression.Call(
            null,
            typeof(Type).GetMethod(nameof(Type.GetType), new []{typeof(string)}) ?? throw DefaultError(),
            Expression.Property(toParameter, nameof(UnitNetData.Type)));

        MethodCallExpression objectFactory = Expression.Call(
            Expression.Constant(FastReflection.Shared),
            typeof(FastReflection).GetMethod(nameof(FastReflection.GetCreator), new[] { typeof(Type), typeof(Type[]) }) ?? throw DefaultError(),
            typeGetter,
            Expression.Constant(new[] { typeof(double), typeof(UnitSystem) }));

        MethodCallExpression targetObject = Expression.Call(
            objectFactory,
            typeof(Func<object[], object>).GetMethod("Invoke") ?? throw DefaultError(),
            Expression.NewArrayInit(
                typeof(object),
                Expression.Convert(
                    Expression.Property(toParameter, nameof(UnitNetData.Value)),
                    typeof(object)),
                Expression.Convert(
                    SiSystem(),
                    typeof(object))));

        return Expression.Convert(targetObject, _from);
    }
}