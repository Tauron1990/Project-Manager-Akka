using System.Linq.Expressions;
using NRules.Fluent.Dsl;
using NRules.RuleModel;
using SpaceConqueror.States;

namespace SpaceConqueror.Rules;

public static class ExpressionExtensions
{
    public static void UpadeState(this IContext context, Func<IState> newState)
        => context.Update(newState());
}