using System.Collections.Concurrent;
using System.Linq.Expressions;
using FastExpressionCompiler;
using JetBrains.Annotations;
using SimpleProjectManager.Server.Data.DataConverters;
using SimpleProjectManager.Server.Data.DataConvertersOld;

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

        BaseExpressionConverter? cons = CreateExcpressionConverter(new EntryKey(typeof(TFrom), typeof(TTo)));

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

    internal BaseExpressionConverter? CreateExcpressionConverter(in EntryKey entryKey)
        => _expressionConverters.GetOrAdd(
            entryKey,
            key =>
            {
                (Type from, Type to) = key;

                return VogenExpressionConverter.TryCreate(to, from)
                   .Match(
                        c => c,
                        _ => SingleValueExpressionConverter.TryCreate(to, from)
                           .Match(
                                c => c,
                                _ => ComplexExpressionConverter.TryCreate(from, to)
                                   .Match<BaseExpressionConverter?>(
                                        c => c,
                                        _ => null)));
            });
}