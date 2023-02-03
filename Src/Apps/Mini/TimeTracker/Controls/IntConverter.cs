using System.Globalization;
using System.Windows.Data;
using Tauron.Application.Wpf.Converter;

namespace TimeTracker.Controls;

public sealed class IntConverter : ValueConverterFactoryBase
{
    protected override IValueConverter Create() => new Con();

    private sealed class Con : StringConverterBase<int>
    {
        protected override bool CanConvertBack
            => true;

        protected override string Convert(int value)
            => value.ToString(CultureInfo.CurrentUICulture);

        protected override int ConvertBack(string value)
            => int.Parse(value, NumberStyles.Any, CultureInfo.CurrentUICulture);
    }
}