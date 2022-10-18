using System.Collections.Concurrent;
using System.Linq.Expressions;
using FastExpressionCompiler;
using JetBrains.Annotations;
using SimpleProjectManager.Server.Data.DataConverters;
using Stl.Generators;

namespace SimpleProjectManager.Server.Data;

public sealed record Converter<TFrom, TTo>(Func<TFrom, TTo> ToDto, Func<TTo, TFrom> FromDto);

public sealed class DataConverter
{
    private readonly ConcurrentDictionary<EntryKey, object> _converters = new();
    private readonly ConcurrentDictionary<EntryKey, BaseExpressionConverter?> _expressionConverters = new();

    public Converter<TFrom, TTo> Get<TFrom, TTo>()
        => (Converter<TFrom, TTo>)_converters.GetOrAdd(
            new EntryKey(typeof(TFrom), typeof(TTo)),
            _ => CreateConverter<TFrom, TTo>());

    private Converter<TFrom, TTo> CreateConverter<TFrom, TTo>()
    {
        ParameterExpression fromToInput = Expression.Parameter(typeof(TFrom));
        ParameterExpression toFromInput = Expression.Parameter(typeof(TTo));

        BaseExpressionConverter? cons = CreateExcpressionConverter(typeof(TFrom), typeof(TTo));

        if(cons is null)
            throw new InvalidOperationException($"No Converter for ({typeof(TFrom)} -- {typeof(TTo)}) combination found");
        
        Expression fromTo = cons.CreateFromTo(fromToInput);
        Expression tofrom = cons.CreateToFrom(toFromInput);

        #if DEBUG
        var fromToString = fromTo.ToCSharpString();
        var tofromstring = tofrom.ToCSharpString();
        #endif

        var fromToFunc = Expression
           .Lambda<Func<TFrom, TTo>>(fromTo, fromToInput)
           .CompileFast();

        var toFromFunc = Expression
           .Lambda<Func<TTo, TFrom>>(tofrom, toFromInput)
           .CompileFast();

        return new Converter<TFrom, TTo>(fromToFunc, toFromFunc);
    }

    private BaseExpressionConverter? CreateExcpressionConverter(Type fromKey, Type toKey)
        => _expressionConverters.GetOrAdd(
            new EntryKey(fromKey, toKey),
            key =>
            {
                (Type from, Type to) = key;
                
                if(VogenExpressionConverter.CanCreate(to, from))
                    return new VogenExpressionConverter(to, from);

                if(SingleValueExpressionConverter.CanCreate(to, from))
                    return new SingleValueExpressionConverter(to, from);

                if(UnitsNetExpressionConverter.CanCreate(from))
                    return new UnitsNetExpressionConverter(from);

                if(ComplexExpressionConverter.TryCreate(from, to, out var converter))
                    return converter;
                
                return null;
            });

    private sealed record EntryKey([UsedImplicitly]Type From, [UsedImplicitly]Type To);
}