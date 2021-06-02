using System.Windows.Data;
using System.Windows.Media;
using Tauron.Application.Wpf.Converter;

namespace TimeTracker.Controls
{
    public sealed class IsValidEntryConverter : ValueConverterFactoryBase
    {
        protected override IValueConverter Create() => CreateCommonConverter<bool, Brush>(b => b ? Brushes.Transparent : Brushes.DarkRed);
    }
}