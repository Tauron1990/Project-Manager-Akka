using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using Tauron.Application.Wpf.Converter;

namespace TimeTracker.Controls
{
    public sealed class DoubleConverter : ValueConverterFactoryBase
    {
        private sealed class _ : StringConverterBase<double>
        {
            protected override string Convert(double value)
                => value.ToString(CultureInfo.CurrentCulture);

            protected override bool CanConvertBack
                => true;

            protected override double ConvertBack(string value)
                => double.Parse(value);
        }

        protected override IValueConverter Create() => new _();
    }

    public sealed class DoubleValidation : MarkupExtension
    {
        public override object ProvideValue(IServiceProvider serviceProvider) => new DoubleValidationRule();
    }

    public sealed class DoubleValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value is not string integer)
                return new ValidationResult(false, "Value ist kein String");

            return double.TryParse(integer, out _) ? ValidationResult.ValidResult : new ValidationResult(false, "Eingabe ist keine komma Zahl");
        }
    }
}