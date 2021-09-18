using System;

namespace Tauron.Grid.Internal
{
    internal static class Converter
    {
        internal static string ToCss(int value, CSSUnit unit)
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

        internal static string ToCss(GridAlignExt ext)
            => ext switch
            {
                GridAlignExt.None => "none",
                GridAlignExt.Start => "start",
                GridAlignExt.End => "end",
                GridAlignExt.Center => "center",
                GridAlignExt.Stretch => "stretch",
                GridAlignExt.SpaceAround => "space-around",
                GridAlignExt.SpaceBetween => "space-between",
                GridAlignExt.SpaceEvenly => "space-evenly",
                _ => throw new ArgumentOutOfRangeException(nameof(ext), ext, null)
            };

        internal static string ToCss(AutoFlow flow)
            => flow switch
            {
                AutoFlow.None => "row",
                AutoFlow.Row => "row",
                AutoFlow.Column => "Column",
                AutoFlow.RowDense => "row dense",
                AutoFlow.ColumnDense => "column dense",
                _ => throw new ArgumentOutOfRangeException(nameof(flow), flow, null)
            };
    }
}