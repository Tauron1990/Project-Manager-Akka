using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using Tauron.Application.Wpf.Converter;

namespace TimeTracker.Controls
{
    public sealed class IntConverter : ValueConverterFactoryBase
    {
        private sealed class _ : StringConverterBase<int>
        {
            protected override string Convert(int value) 
                => value.ToString();

            protected override bool CanConvertBack 
                => true;

            protected override int ConvertBack(string value) 
                => int.Parse(value);
        }

        protected override IValueConverter Create() => new _();
    }

    public sealed class IntValidation : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value is not string integer)
                return new ValidationResult(false, "Value ist kein String");

            return int.TryParse(integer, out _) ? ValidationResult.ValidResult : new ValidationResult(false, "Eingabe ist keine ganze Zahl");
        }
    }
}