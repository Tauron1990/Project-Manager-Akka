﻿using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using JetBrains.Annotations;
using Tauron.Application.CommonUI.Helper;
using Tauron.Application.CommonUI.UI;
using Tauron.Application.Wpf.AppCore;

namespace Tauron.Application.Wpf.UI;

[PublicAPI]
public class ActorBinding : BindingDecoratorBase
{
    public ActorBinding(string name) => RootProperty = name;

    public ActorBinding() => RootProperty = string.Empty;
    public string RootProperty { get; set; }

    public int Delay
    {
        get => Binding.Delay;
        set => Binding.Delay = value;
    }

    public ValidationRule ValidationRule
    {
        set => ValidationRules?.Add(value);
    }

    public override object? ProvideValue(IServiceProvider provider)
    {
        try
        {
            if(!TryGetTargetItems(provider, out DependencyObject? dependencyObject, out _))
                return DependencyProperty.UnsetValue;

            if(DesignerProperties.GetIsInDesignMode(dependencyObject))
                return DependencyProperty.UnsetValue;

            if(!ControlBindLogic.FindDataContext(ElementMapper.Create(dependencyObject).AsOption(), out DataContextPromise? model))
                return null;

            Path = Path != null
                ? new PropertyPath("Value." + Path.Path, Path.PathParameters)
                : new PropertyPath("Value");
            Source = new DeferredSource(RootProperty, model);
            if(Binding.Delay < 200)
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