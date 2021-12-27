using System;

namespace Tauron.Grid
{
    public enum GridPositionMode
    {
        None = 0,
        Auto,
        Span,
        NoSpan
    }

    public readonly struct GridPoint
    {
        public GridPositionMode Mode { get; }

        public string? Name { get; }

        public int Position { get; }

        public GridPoint(GridPositionMode mode, string? name, int position)
        {
            Mode = mode;
            Name = name;
            Position = position;
        }

        private static GridPositionMode ToMode(bool span)
            => span ? GridPositionMode.Span : GridPositionMode.NoSpan;

        public static GridPoint Auto()
            => new(GridPositionMode.Auto, null, -1);

        public static GridPoint ToName(string name, bool withSpan)
            => new(ToMode(withSpan), name, -1);

        public static GridPoint ToPosition(int position, bool withSpan)
            => new(ToMode(withSpan), null, position);

        public static implicit operator GridPoint(int pos) => ToPosition(pos, withSpan: false);
        public static implicit operator GridPoint(string name) => ToName(name, withSpan: false);

        public override string ToString()
            => Mode switch
            {
                GridPositionMode.Auto => "auto",
                GridPositionMode.Span => $"span {GetValue()}",
                GridPositionMode.NoSpan => GetValue(),
                _ => throw new InvalidCastException("Invalid Grid Position Mode")
            };

        private string GetValue()
            => string.IsNullOrWhiteSpace(Name) ? Position.ToString() : Name;
    }
}