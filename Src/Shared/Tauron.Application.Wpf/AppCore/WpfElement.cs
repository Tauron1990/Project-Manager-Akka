using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows;
using Tauron.Application.CommonUI;

namespace Tauron.Application.Wpf.AppCore;

public class WpfElement : WpfObject, IUIElement
{
    private readonly FrameworkElement _element;

    public WpfElement(FrameworkElement element)
        : base(element)
        => _element = element;

    public object? DataContext
    {
        get => _element.DataContext;
        set => _element.DataContext = value;
    }

    public IObservable<object> DataContextChanged
        => Observable
           .FromEvent<DependencyPropertyChangedEventHandler, DependencyPropertyChangedEventArgs>(
                h => _element.DataContextChanged += h,
                h => _element.DataContextChanged -= h)
           .Select(c => c.NewValue);

    public IObservable<Unit> Loaded => Observable
       .FromEventPattern<RoutedEventHandler, RoutedEventArgs>(h => _element.Loaded += h, h => _element.Loaded -= h)
       .ToUnit();

    public IObservable<Unit> Unloaded => Observable
       .FromEventPattern<RoutedEventHandler, RoutedEventArgs>(
            h => _element.Unloaded += h,
            h => _element.Unloaded -= h).ToUnit();
}

public sealed class WpfContentElement : WpfObject, IUIElement
{
    private readonly FrameworkContentElement _element;

    public WpfContentElement(FrameworkContentElement element)
        : base(element)
        => _element = element;

    public object? DataContext
    {
        get => _element.DataContext;
        set => _element.DataContext = value;
    }

    public IObservable<object> DataContextChanged
        => Observable
           .FromEvent<DependencyPropertyChangedEventHandler, DependencyPropertyChangedEventArgs>(
                h => _element.DataContextChanged += h,
                h => _element.DataContextChanged -= h)
           .Select(c => c.NewValue);

    public IObservable<Unit> Loaded => Observable
       .FromEvent<RoutedEventHandler, RoutedEventArgs>(h => _element.Loaded += h, h => _element.Loaded -= h)
       .ToUnit();

    public IObservable<Unit> Unloaded => Observable
       .FromEvent<RoutedEventHandler, RoutedEventArgs>(h => _element.Unloaded += h, h => _element.Unloaded -= h)
       .ToUnit();
}