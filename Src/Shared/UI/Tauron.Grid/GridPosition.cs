using MudBlazor.Utilities;
using Tauron.Grid.Internal;

namespace Tauron.Grid
{
    public readonly struct GridPosition
    {
        public GridPoint ColumnStart { get; }

        public GridPoint ColumnEnd { get; }

        public GridPoint RowStart { get; }

        public GridPoint RowEnd { get; }

        public GridPosition(GridPoint columnStart, GridPoint columnEnd, GridPoint rowStart, GridPoint rowEnd)
        {
            ColumnStart = columnStart;
            ColumnEnd = columnEnd;
            RowStart = rowStart;
            RowEnd = rowEnd;
        }

        public static GridPosition Empty => new();

        public GridPosition Row(in GridPoint start, in GridPoint end)
            => new(ColumnStart, ColumnEnd, start, end);

        public GridPosition Column(in GridPoint start, in GridPoint end)
            => new(start, end, RowStart, RowEnd);

        public GridPosition Row(in GridPoint start)
            => new(ColumnStart, ColumnEnd, start, RowEnd);

        public GridPosition Column(in GridPoint start)
            => new(start, ColumnEnd, RowStart, RowEnd);

        public StyleBuilder Build(StyleBuilder styleBuilder)
            => styleBuilder.AddStyle("grid-column-start", ColumnStart).AddStyle("grid-column-end", ColumnEnd)
               .AddStyle("grid-row-start", RowStart).AddStyle("grid-row-end", RowEnd);

        public override string ToString()
            => $"{RowStart} / {ColumnStart} / {RowEnd} / {ColumnEnd}";
    }
}