using System;
using System.Reactive;

namespace Tauron.Application.CommonUI;

public interface IUIElement : IUIObject
{
    object? DataContext { get; set; }
    IObservable<object> DataContextChanged { get; }
    IObservable<Unit> Loaded { get; }
    IObservable<Unit> Unloaded { get; }
}