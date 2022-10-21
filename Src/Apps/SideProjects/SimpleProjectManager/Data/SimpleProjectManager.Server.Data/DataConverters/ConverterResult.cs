using OneOf;
using OneOf.Types;

namespace SimpleProjectManager.Server.Data.DataConverters;

[GenerateOneOf]
internal sealed class ConverterResult : OneOfBase<IConverterExpression, None>
{
    private ConverterResult(OneOf<IConverterExpression, None> input) : base(input) { }
    
    
    public static ConverterResult From(IConverterExpression exp) => new(OneOf<IConverterExpression, None>.FromT0(exp));

    public static ConverterResult None() => new(default(None));
}