using System.Globalization;
using System.Windows.Data;
using Tauron.Application.Wpf.Converter;

namespace TimeTracker.Controls;

public sealed class DoubleConverter : ValueConverterFactoryBase
{
    protected override IValueConverter Create() => new Con();

    private sealed class Con : StringConverterBase<double>
    {
        protected override bool CanConvertBack
            => true;

        protected override string Convert(double value)
            => value.ToString(CultureInfo.CurrentCulture);

        protected override double ConvertBack(string value)
            => double.Parse(value, NumberStyles.Any, CultureInfo.CurrentUICulture);
    }
}