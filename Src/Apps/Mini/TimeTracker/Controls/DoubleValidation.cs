using System;
using System.Windows.Markup;

namespace TimeTracker.Controls
{
    public sealed class DoubleValidation : MarkupExtension
    {
        public override object ProvideValue(IServiceProvider serviceProvider) => new DoubleValidationRule();
    }
}