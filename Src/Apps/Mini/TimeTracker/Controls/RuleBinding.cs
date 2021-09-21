using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Tauron.Application.Wpf.UI;

namespace TimeTracker.Controls
{
    public sealed class RuleBinding : BindingDecoratorBase
    {
        public RuleBinding(string path) => Path = new PropertyPath(path);

        public ValidationRule Rule
        {
            set => ValidationRules?.Add(value);
        }

        public override object? ProvideValue(IServiceProvider provider)
        {
            Binding.Delay = 200;
            Binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;

            return base.ProvideValue(provider);
        }
    }
}