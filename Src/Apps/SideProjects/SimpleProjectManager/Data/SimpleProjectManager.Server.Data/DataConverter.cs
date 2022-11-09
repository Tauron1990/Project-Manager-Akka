using System.Linq.Expressions;
using FastExpressionCompiler;
using SimpleProjectManager.Server.Data.DataConverters;

namespace SimpleProjectManager.Server.Data;

public sealed record Converter<TFrom, TTo>(Func<TFrom, TTo> ToDto, Func<TTo, TFrom> FromDto);

public sealed class DataConverter
{
    private readonly ConverterDictionary _converters = new();
    private readonly ExpressionDictionary _expressionConverters = new();

    public Converter<TFrom, TTo> Get<TFrom, TTo>()
        => _converters.Get(CreateConverter<TFrom, TTo>);

    private Converter<TFrom, TTo> CreateConverter<TFrom, TTo>()
    {
        ParameterExpression fromToInput = Expression.Parameter(typeof(TFrom));
        ParameterExpression toFromInput = Expression.Parameter(typeof(TTo));

        IConverterExpression? converterFromTo = _expressionConverters.CreateExcpressionConverter(this, new EntryKey(typeof(TFrom), typeof(TTo)));
        IConverterExpression? converterToFrom = _expressionConverters.CreateExcpressionConverter(this, new EntryKey(typeof(TTo), typeof(TFrom)));


        if(converterFromTo is null)
            throw new InvalidOperationException($"No Converter for ({typeof(TFrom)} -- {typeof(TTo)}) combination found");
        if(converterToFrom is null)
            throw new InvalidOperationException($"No Converter for ({typeof(TTo)} -- {typeof(TFrom)}) combination found");

        Expression fromTo = converterFromTo.Generate(fromToInput);
        Expression tofrom = converterToFrom.Generate(toFromInput);

        #if DEBUG
        string? fromToString = fromTo.ToCSharpString();
        string? tofromstring = tofrom.ToCSharpString();
        #endif

        var fromToFunc = Expression
           .Lambda<Func<TFrom, TTo>>(fromTo, fromToInput)
           .CompileFast();

        var toFromFunc = Expression
           .Lambda<Func<TTo, TFrom>>(tofrom, toFromInput)
           .CompileFast();

        return new Converter<TFrom, TTo>(fromToFunc, toFromFunc);
    }

    internal IConverterExpression? CreateExcpressionConverter(in EntryKey entryKey)
        => _expressionConverters.CreateExcpressionConverter(this, entryKey);
}