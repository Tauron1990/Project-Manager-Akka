using System.Linq.Expressions;
using NRules.Fluent.Dsl;

namespace SpaceConqueror.States.GameTime;

public static class GameTimeExtensions
{
    public static ILeftHandSideExpression MatchGameTime(this ILeftHandSideExpression expression, Expression<Func<GameTime>> gametime, params Expression<Func<GameTime, bool>>[] conditions)
        => expression.Match(gametime, conditions);
}