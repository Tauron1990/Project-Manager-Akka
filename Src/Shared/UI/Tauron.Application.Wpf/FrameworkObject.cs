#region

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Media;
using JetBrains.Annotations;
using Stl;
using Tauron.Application.CommonUI;
using Tauron.Application.CommonUI.Helper;
using Tauron.Application.Wpf.AppCore;

#endregion

#pragma warning disable EPS06

namespace Tauron.Application.Wpf;

/// <summary>The framework object.</summary>
[DebuggerStepThrough]
[PublicAPI]
public sealed class FrameworkObject : IInternalWeakReference
{
    private readonly Option<ElementReference<FrameworkContentElement>> _fce;

    private readonly Option<ElementReference<FrameworkElement>> _fe;

    private readonly bool _isFce;

    private readonly bool _isFe;

    public FrameworkObject(object? obj, bool isWeak = true)
    {
        var fe = obj as FrameworkElement;
        var fce = obj as FrameworkContentElement;

        _isFe = fe != null;
        _isFce = fce != null;
        IsValid = _isFce || _isFe;

        if(fe != null) _fe = new ElementReference<FrameworkElement>(fe, isWeak);
        else if(fce != null) _fce = new ElementReference<FrameworkContentElement>(fce, isWeak);
    }

    public object? DataContext
    {
        get
        {
            if(!IsValid) return null;

            if(TryGetFrameworkElement(out FrameworkElement? fe)) return fe.DataContext;

            return TryGetFrameworkContentElement(out FrameworkContentElement? fce) ? fce.DataContext : null;
        }

        set
        {
            if(!IsValid) return;

            if(TryGetFrameworkElement(out FrameworkElement? fe)) fe.DataContext = value;
            else if(TryGetFrameworkContentElement(out FrameworkContentElement? fce)) fce.DataContext = value;
        }
    }

    public bool IsValid { get; }

    public Option<DependencyObject> Original
    {
        get
        {
            if(_isFe) return _fe.Value.Target.Value;

            return _isFce ? _fce.Value.Target.Value : Option<DependencyObject>.None;
        }
    }

    public Option<DependencyObject> Parent
    {
        get
        {
            if(!IsValid) return Option<DependencyObject>.None;

            if(TryGetFrameworkElement(out FrameworkElement? fe)) return fe.Parent.OptionNotNull();

            return TryGetFrameworkContentElement(out FrameworkContentElement? fce) ? fce.Parent.OptionNotNull() : Option<DependencyObject>.None;
        }
    }

    public Option<DependencyObject> VisualParent
    {
        get
        {
            if(!IsValid) return Option<DependencyObject>.None;

            return TryGetFrameworkElement(out FrameworkElement? fe) ? VisualTreeHelper.GetParent(fe).OptionNotNull() : Option<DependencyObject>.None;
        }
    }

    bool IInternalWeakReference.IsAlive
    {
        get
        {
            if(_isFe) return _fe.Value.IsAlive;

            return _isFce && _fce.Value.IsAlive;
        }
    }

    public IUIObject? CreateElement()
    {
        if(TryGetFrameworkElement(out FrameworkElement? el))
            return ElementMapper.Create(el);

        return TryGetFrameworkContentElement(out FrameworkContentElement? ce) ? ElementMapper.Create(ce) : null;
    }

    public event DependencyPropertyChangedEventHandler DataContextChanged
    {
        add
        {
            if(!IsValid) return;

            if(TryGetFrameworkElement(out FrameworkElement? fe)) fe.DataContextChanged += value;
            else if(TryGetFrameworkContentElement(out FrameworkContentElement? fce)) fce.DataContextChanged += value;
        }

        remove
        {
            if(!IsValid) return;

            if(TryGetFrameworkElement(out FrameworkElement? fe)) fe.DataContextChanged -= value;
            else if(TryGetFrameworkContentElement(out FrameworkContentElement? fce)) fce.DataContextChanged -= value;
        }
    }

    public event RoutedEventHandler LoadedEvent
    {
        add
        {
            if(!IsValid) return;

            if(TryGetFrameworkElement(out FrameworkElement? fe)) fe.Loaded += value;
            else if(TryGetFrameworkContentElement(out FrameworkContentElement? fce)) fce.Loaded += value;
        }

        remove
        {
            if(!IsValid) return;

            if(TryGetFrameworkElement(out FrameworkElement? fe)) fe.Loaded -= value;
            else if(TryGetFrameworkContentElement(out FrameworkContentElement? fce)) fce.Loaded -= value;
        }
    }

    public bool TryGetFrameworkContentElement([NotNullWhen(true)] out FrameworkContentElement? contentElement)
    {
        contentElement = _isFce ? _fce.Value.Target.Value : null;

        return contentElement != null;
    }

    public bool TryGetFrameworkElement([NotNullWhen(true)] out FrameworkElement? frameworkElement)
    {
        FrameworkElement? temp = _isFe ? _fe.Value.Target.Value : null;

        if(temp is null)
        {
            frameworkElement = null;

            return false;
        }

        frameworkElement = temp;

        return true;
    }

    [DebuggerStepThrough]
    private class ElementReference<TReference> : IInternalWeakReference
        where TReference : class
    {
        private readonly Option<TReference> _reference;

        private readonly Option<WeakReference<TReference>> _weakRef;

        internal ElementReference(TReference reference, bool isWeak)
        {
            if(isWeak) _weakRef = new WeakReference<TReference>(reference);
            else _reference = reference;
        }

        internal Option<TReference> Target => _weakRef.Select(v => v.TypedTarget()).GetOrElse(_reference);

        public bool IsAlive => _weakRef.HasValue && _weakRef.Value.IsAlive();
    }
}