//HintName: TestNamespace.g.cs
namespace TestNamespace;
public static partial class TestName
{
    public static string ToStringFast(this Colour value)
        => value switch
        {
            Colour.Red => nameof(Colour.Red),
            Colour.Blue => nameof(Colour.Blue),
            _ => value.ToString(),
        };
}
