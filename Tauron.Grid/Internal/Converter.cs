namespace Tauron.Grid.Internal
{
    static class Converter
    {
        public static string ToCss(int value, CSSUnit unit)
            => unit switch
            {
                CSSUnit.Pixel => $"{value}px",
                CSSUnit.Milimeter => $"{value}mm",
                CSSUnit.Centimeter => $"{value}cm",
                CSSUnit.ViewportWidth => $"{value}vw",
                CSSUnit.ViewportHeight => $"{value}vh",
                CSSUnit.Percentage => $"{value}%",
                _ => $"{value}fr"
            };
    }
}