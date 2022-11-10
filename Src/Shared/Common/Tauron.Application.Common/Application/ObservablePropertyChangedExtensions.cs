using System;
using System.Linq.Expressions;
using System.Reactive.Linq;
using FastExpressionCompiler;
using JetBrains.Annotations;

namespace Tauron.Application;

[PublicAPI]
public static class ObservablePropertyChangedExtensions
{
    public static IObservable<TProp> WhenAny<TProp>(this IObservablePropertyChanged @this, Expression<Func<TProp>> prop)
    {
        string name = Reflex.PropertyName(prop);
        var func = prop.CompileFast();

        return @this.PropertyChangedObservable.Where(propName => string.Equals(propName, name, StringComparison.Ordinal)).Select(_ => func());
    }
}