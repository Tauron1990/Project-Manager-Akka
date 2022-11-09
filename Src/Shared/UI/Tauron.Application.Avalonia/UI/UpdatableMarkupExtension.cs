using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using JetBrains.Annotations;

namespace Tauron.Application.Avalonia.UI;

[PublicAPI]
public abstract class UpdatableMarkupExtension : MarkupExtension
{
    protected object? TargetObject { get; private set; }

    protected object? TargetProperty { get; private set; }

    public sealed override object ProvideValue(IServiceProvider serviceProvider)
    {
        if(serviceProvider.GetService(typeof(IProvideValueTarget)) is IProvideValueTarget service)
        {
            if(service.TargetObject.GetType().FullName == "System.Windows.SharedDp")
                return this;

            TargetObject = service.TargetObject;
            TargetProperty = service.TargetProperty;
        }

        return ProvideValueInternal(serviceProvider);
    }

    protected void UpdateValue(object? value)
    {
        if(TargetObject != null)
        {
            if(TargetProperty is AvaloniaProperty dependencyProperty)
            {
                var obj = TargetObject as AvaloniaObject;

                void UpdateAction()
                {
                    obj.SetValue(dependencyProperty, value);
                }

                // Check whether the target object can be accessed from the
                // current thread, and use Dispatcher.Invoke if it can't

                if(obj?.CheckAccess() == true)
                    UpdateAction();
                else if(obj != null)
                    Dispatcher.UIThread.InvokeAsync(UpdateAction, DispatcherPriority.Background)
                       .Ignore();
            }
            else // _targetProperty is PropertyInfo
            {
                var prop = TargetProperty as PropertyInfo;
                prop?.SetValue(TargetObject, value, null);
            }
        }
    }

    protected virtual bool TryGetTargetItems(
        IServiceProvider? provider,
        [NotNullWhen(true)] out AvaloniaObject? target, [NotNullWhen(true)] out AvaloniaProperty? dp)
    {
        target = null;
        dp = null;

        //create a binding and assign it to the target
        if(!(provider?.GetService(typeof(IProvideValueTarget)) is IProvideValueTarget service)) return false;

        //we need dependency objects / properties
        target = service.TargetObject as AvaloniaObject;
        dp = service.TargetProperty as AvaloniaProperty;

        return target != null && dp != null;
    }

    protected abstract object ProvideValueInternal(IServiceProvider serviceProvider);
}