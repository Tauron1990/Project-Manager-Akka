using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using Akka.Util.Extensions;
using JetBrains.Annotations;
using Tauron.Application.CommonUI.Helper;
using Tauron.Application.CommonUI.UI;
using Tauron.Application.Wpf.AppCore;

namespace Tauron.Application.Wpf.UI
{
    [PublicAPI]
    public class ActorBinding : BindingDecoratorBase
    {
        private readonly string _name;

        public ActorBinding(string name) => _name = name;

        public override object? ProvideValue(IServiceProvider provider)
        {
            try
            {
                if (!TryGetTargetItems(provider, out var dependencyObject, out _))
                    return DependencyProperty.UnsetValue;

                if (DesignerProperties.GetIsInDesignMode(dependencyObject))
                    return DependencyProperty.UnsetValue;

                if (!ControlBindLogic.FindDataContext(ElementMapper.Create(dependencyObject).AsOption(), out var model))
                    return null;

                Path = Path != null
                    ? new PropertyPath("Value." + Path.Path, Path.PathParameters)
                    : new PropertyPath("Value");
                Source = new DeferredSource(_name, model);
                Binding.Delay = 200;
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                Binding.ValidatesOnNotifyDataErrors = true;

                return base.ProvideValue(provider);
            }
            catch (NullReferenceException)
            {
                return null;
            }
        }
    }
}