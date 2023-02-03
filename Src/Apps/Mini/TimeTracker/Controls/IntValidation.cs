using System;
using System.Windows.Markup;

namespace TimeTracker.Controls
{
    public sealed class IntValidation : MarkupExtension
    {
        public override object ProvideValue(IServiceProvider serviceProvider) => new IntValidationRule();
    }
}