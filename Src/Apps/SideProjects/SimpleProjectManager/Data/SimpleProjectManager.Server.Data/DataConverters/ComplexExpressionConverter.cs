using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace SimpleProjectManager.Server.Data.DataConverters;

public sealed class ComplexExpressionConverter : BaseExpressionConverter
{
    public static bool TryCreate(Type from, Type to, [MaybeNullWhen(false)]out ComplexExpressionConverter converter)
    {
        
    }

    public override Expression CreateFromTo(ParameterExpression fromParamameter)
        => throw new NotImplementedException();

    public override Expression CreateToFrom(ParameterExpression toParameter)
        => throw new NotImplementedException();
}