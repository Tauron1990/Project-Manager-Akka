using Tauron.Grid.Internal;

namespace Tauron.Grid;

public readonly struct Lenght
{
    public bool Set { get; }

    public int Value { get; }

    public CssUnit Unit { get; }

    public Lenght(int value, CssUnit unit)
    {
        Value = value;
        Unit = unit;
        Set = true;
    }

    public static Lenght FromFraction(int value)
        => new(value, CssUnit.Fraction);

    public static Lenght FromPercentage(int value)
        => new(value, CssUnit.Percentage);

    public static Lenght FromViewportHeight(int value)
        => new(value, CssUnit.Percentage);

    public static Lenght FromViewportWidth(int value)
        => new(value, CssUnit.ViewportWidth);

    public static Lenght FromPixel(int value)
        => new(value, CssUnit.Pixel);

    public static Lenght FromCentimeter(int value)
        => new(value, CssUnit.Centimeter);

    public static Lenght FromMilimeter(int value)
        => new(value, CssUnit.Milimeter);

    public override string ToString() => Set ? Converter.ToCss(Value, Unit) : "none";
}