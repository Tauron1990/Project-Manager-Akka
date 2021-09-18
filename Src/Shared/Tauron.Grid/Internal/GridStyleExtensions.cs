using MudBlazor.Utilities;

namespace Tauron.Grid.Internal
{
    public static class GridStyleExtensions
    {
        public static StyleBuilder AddStyle(this StyleBuilder styleBuilder, string name, Lenght lenght)
            => styleBuilder.AddStyle(name, lenght.ToString, lenght.Set);

        public static StyleBuilder AddStyle(this StyleBuilder styleBuilder, string name, GridAlign align)
            => styleBuilder.AddStyle(name, () => align.ToString().ToLower(), align != GridAlign.None);

        public static StyleBuilder AddStyle(this StyleBuilder styleBuilder, string name, GridAlignExt align)
            => styleBuilder.AddStyle(name, () => Converter.ToCss(align), align != GridAlignExt.None);

        public static StyleBuilder AddStyle(this StyleBuilder styleBuilder, string name, AutoFlow autoFlow)
            => styleBuilder.AddStyle(name, Converter.ToCss(autoFlow), autoFlow != AutoFlow.None);

        public static StyleBuilder AddStyle(this StyleBuilder styleBuilder, string name, in GridPoint point)
            => styleBuilder.AddStyle(name, point.ToString, point.Mode != GridPositionMode.None);
    }
}