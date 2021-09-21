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
        protected override IValueConverter Create() => new Con();

        private sealed class Con : StringConverterBase<double>
        {
            protected override bool CanConvertBack
                => true;

            protected override string Convert(double value)
                => value.ToString(CultureInfo.CurrentCulture);

            protected override double ConvertBack(string value)
                => double.Parse(value);
        }
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
                return new ValidationResult(isValid: false, "Value ist kein String");

            return double.TryParse(integer, out _) ? ValidationResult.ValidResult : new ValidationResult(isValid: false, "Eingabe ist keine komma Zahl");
        }
    }
}