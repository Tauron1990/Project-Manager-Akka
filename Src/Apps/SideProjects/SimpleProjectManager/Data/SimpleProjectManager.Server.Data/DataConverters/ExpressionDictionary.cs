using System.Collections.Concurrent;

namespace SimpleProjectManager.Server.Data.DataConverters;

internal sealed class ExpressionDictionary
{
    private readonly ConcurrentDictionary<EntryKey, IConverterExpression?> _expressionConverters = new();
    
    internal IConverterExpression? CreateExcpressionConverter(DataConverter dataConverter, in EntryKey entryKey)
        => _expressionConverters.GetOrAdd(
            entryKey,
            static (key, converter) =>
            {
                (Type from, Type to) = key;

                if(UnitsNetExpression.TryCreate(from, to).TryPickT0(out IConverterExpression converter1, out _))
                    return converter1;

                if(VogenExpression.TryCreate(from, to).TryPickT0(out IConverterExpression converter2, out _))
                    return converter2;

                if(SingleValueExpression.TryCreate(from, to).TryPickT0(out IConverterExpression converter3, out _))
                    return converter3;

                if(ComplexExpression.TryCreate(from, to, converter).TryPickT0(out IConverterExpression converter4, out _))
                    return converter4;

                return null;
            }, dataConverter);
}