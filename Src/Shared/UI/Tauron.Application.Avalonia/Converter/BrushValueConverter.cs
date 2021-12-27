using System;
using Avalonia.Data.Converters;
using Avalonia.Media;
using JetBrains.Annotations;

namespace Tauron.Application.Avalonia.Converter
{
    [PublicAPI]
    public sealed class BrushValueConverter : ValueConverterFactoryBase
    {
        protected override IValueConverter Create() => new Converter();

        private class Converter : ValueConverterBase<string, IBrush>
        {
            private static readonly BrushConverter ConverterImpl = new();

            protected override IBrush Convert(string value)
            {
                try
                {
                    if (ConverterImpl.ConvertFrom(value) is Brush brush)
                        return brush;

                    return Brushes.Black;
                }
                catch (FormatException)
                {
                    return Brushes.Black;
                }
            }
        }
    }
}