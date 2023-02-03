using System.Globalization;
using System.Windows.Controls;

namespace TimeTracker.Controls;

public sealed class DoubleValidationRule : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        if (value is not string integer)
            return new ValidationResult(isValid: false, "Value ist kein String");

        return double.TryParse(integer, NumberStyles.Any, CultureInfo.CurrentUICulture, out _) ? ValidationResult.ValidResult : new ValidationResult(isValid: false, "Eingabe ist keine komma Zahl");
    }
}