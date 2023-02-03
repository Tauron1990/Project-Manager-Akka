using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using JetBrains.Annotations;

namespace Tauron.Application.Wpf.Converter;

[PublicAPI]
[MarkupExtensionReturnType(typeof(IValueConverter))]
public class StringToIntConverter : ValueConverterFactoryBase
{
    protected override IValueConverter Create() => new Converter();

    private class Converter : StringConverterBase<int>
    {
        protected override bool CanConvertBack => true;

        protected override string Convert(int value) => value.ToString(CultureInfo.InvariantCulture);

        protected override int ConvertBack(string value)
        {
            if(string.IsNullOrEmpty(value))
                return 0;

            try
            {
                return int.Parse(value, NumberStyles.Any, CultureInfo.InvariantCulture);
            }
            catch (Exception e) when (e is ArgumentException or FormatException or OverflowException)
            {
                return 0;
            }
        }
    }
}