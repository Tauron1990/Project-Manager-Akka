using System;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;

namespace Tauron;

[PublicAPI]
public static class Reflex
{
    public static MethodInfo MethodInfo<T>(Expression<Action<T>> expression)
    {
        if(expression.Body is MethodCallExpression member)
            return member.Method;

        throw new ArgumentException("Expression is not a method", nameof(expression));
    }

    public static MethodInfo MethodInfo(Expression<Action> expression)
    {
        if(expression.Body is MethodCallExpression member)
            return member.Method;

        throw new ArgumentException("Expression is not a method", nameof(expression));
    }

    public static string PropertyName<T>(Expression<Func<T>> propertyExpression)
    {
        var memberExpression = (MemberExpression)propertyExpression.Body;

        return memberExpression.Member.Name;
    }

    public static string PropertyName<TIn, T>(Expression<Func<TIn, T>> propertyExpression)
    {
        var memberExpression = (MemberExpression)propertyExpression.Body;

        return memberExpression.Member.Name;
    }
}