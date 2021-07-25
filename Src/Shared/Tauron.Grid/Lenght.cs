using Tauron.Grid.Internal;

namespace Tauron.Grid
{
    public readonly struct Lenght
    {
        public bool Set { get; }

        public int Value { get; }

        public CSSUnit Unit { get; }

        public Lenght(int value, CSSUnit unit)
        {
            Value = value;
            Unit = unit;
            Set = true;
        }
        
        public static Lenght FromFraction(int value) 
            => new(value, CSSUnit.Fraction);

        public static Lenght FromPercentage(int value)
            => new(value, CSSUnit.Percentage);

        public static Lenght FromViewportHeight(int value)
            => new(value, CSSUnit.Percentage);

        public static Lenght FromViewportWidth(int value)
            => new(value, CSSUnit.ViewportWidth);

        public static Lenght FromPixel(int value)
            => new(value, CSSUnit.Pixel);

        public static Lenght FromCentimeter(int value)
            => new(value, CSSUnit.Centimeter);

        public static Lenght FromMilimeter(int value)
            => new(value, CSSUnit.Milimeter);

        public override string ToString() => Set ? Converter.ToCss(Value, Unit) : "none";
    }
}