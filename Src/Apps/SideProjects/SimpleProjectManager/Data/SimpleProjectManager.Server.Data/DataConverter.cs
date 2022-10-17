using System.Collections.Concurrent;
using System.Linq.Expressions;
using FastExpressionCompiler;
using SimpleProjectManager.Server.Data.DataConverters;
using Stl.Generators;

namespace SimpleProjectManager.Server.Data;

public sealed record Converter<TFrom, TTo>(Func<TFrom, TTo> ToDto, Func<TTo, TFrom> FromDto);

public sealed class DataConverter
{
    private readonly ConcurrentDictionary<EntryKey, object> _converters = new();

    public Converter<TFrom, TTo> Get<TFrom, TTo>()
        => (Converter<TFrom, TTo>)_converters.GetOrAdd(
            new EntryKey(typeof(TFrom), typeof(TTo)),
            _ => CreateConverter<TFrom, TTo>());

    private Converter<TFrom, TTo> CreateConverter<TFrom, TTo>()
    {
        Expression? fromTo = null;
        Expression? tofrom = null;

        ParameterExpression fromToInput = Expression.Parameter(typeof(TFrom));
        ParameterExpression toFromInput = Expression.Parameter(typeof(TTo));
        
        if(VogenConverter<TFrom, TTo>.CanCreate())
        {
            var cons = new VogenConverter<TFrom, TTo>();

            fromTo = cons.CreateFromTo(fromToInput);
            tofrom = cons.CreateToFrom(toFromInput);
        }
        
        if(fromTo is null || tofrom is null)
            throw new InvalidOperationException("IncOmpatible Type Supplyed");

        var fromToFunc = Expression
           .Lambda<Func<TFrom, TTo>>(fromTo, fromToInput)
           .CompileFast();

        var toFromFunc = Expression
           .Lambda<Func<TTo, TFrom>>(tofrom, toFromInput)
           .CompileFast();

        return new Converter<TFrom, TTo>(fromToFunc, toFromFunc);
    }

    private sealed record EntryKey(Type From, Type To);
}