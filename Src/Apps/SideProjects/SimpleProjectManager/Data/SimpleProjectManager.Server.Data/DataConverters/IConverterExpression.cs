using System.Linq.Expressions;

namespace SimpleProjectManager.Server.Data.DataConverters;

internal interface IConverterExpression
{
    Expression Generate(Expression from);
}