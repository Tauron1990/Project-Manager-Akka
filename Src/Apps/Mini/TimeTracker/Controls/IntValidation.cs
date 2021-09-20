using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using Tauron.Application.Wpf.Converter;

namespace TimeTracker.Controls
{
    public sealed class IntConverter : ValueConverterFactoryBase
    {
        private sealed class Con : StringConverterBase<int>
        {
            protected override string Convert(int value) 
                => value.ToString();

            protected override bool CanConvertBack 
                => true;

            protected override int ConvertBack(string value) 
                => int.Parse(value);
        }

        protected override IValueConverter Create() => new Con();
    }


    public sealed class IntValidation : MarkupExtension
    {
        public override object ProvideValue(IServiceProvider serviceProvider) => new IntValidationRule();
    }

    public sealed class IntValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value is not string integer)
                return new ValidationResult(isValid: false, "Value ist kein String");

            return int.TryParse(integer, out var intData)
                ? intData > 0 
                    ? ValidationResult.ValidResult 
                    : new ValidationResult(isValid: false, "Zahl muss Positive sein") 
                : new ValidationResult(isValid: false, "Eingabe ist keine ganze Zahl");
        }
    }
}