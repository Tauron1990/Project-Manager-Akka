using System.Windows.Data;
using System.Windows.Media;
using Tauron.Application.Wpf.Converter;
using TimeTracker.Data;

namespace TimeTracker.Controls
{
    public sealed class MonthStateToBrushConverter : ValueConverterFactoryBase
    {
        protected override IValueConverter Create() => CreateCommonConverter<MonthState, Brush>(s => s switch
        {
            MonthState.Ok => Brushes.Green,
            MonthState.Short => Brushes.DarkOrange,
            MonthState.Minus => Brushes.Red,
            _ => Brushes.Gray
        });
    }
}