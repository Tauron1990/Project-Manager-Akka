using System.Linq.Expressions;
using NRules.Fluent.Dsl;

namespace SpaceConqueror.States.GameTime;

public static class GameTimeExtensions
{
    public static ILeftHandSideExpression MatchGameTime(this ILeftHandSideExpression expression, Expression<Func<GameTime>> gametime)
        => expression.Match(gametime);
}