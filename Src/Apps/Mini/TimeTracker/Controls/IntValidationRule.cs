using System.Globalization;
using System.Windows.Controls;

namespace TimeTracker.Controls;

public sealed class IntValidationRule : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        if (value is not string integer)
            return new ValidationResult(isValid: false, "Value ist kein String");

        return int.TryParse(integer, NumberStyles.Any, CultureInfo.CurrentUICulture, out int intData)
            ? intData > 0
                ? ValidationResult.ValidResult
                : new ValidationResult(isValid: false, "Zahl muss Positive sein")
            : new ValidationResult(isValid: false, "Eingabe ist keine ganze Zahl");
    }
}