using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia;
using Tauron.Application.CommonUI;

namespace Tauron.Application.Avalonia.AppCore;

public sealed class AvaloniaWindow : AvaloniaElement, IWindow
{
    private readonly global::Avalonia.Controls.Window _window;

    public AvaloniaWindow(global::Avalonia.Controls.Window window)
        : base(window)
        => _window = window;

    public void Show()
    {
        _window.Show();
    }

    public void Hide()
    {
        _window.Hide();
    }

    public Task<bool?> ShowDialog() => _window.ShowDialog<bool?>((Window)_window.Owner);

    public override IObservable<Unit> Loaded
        => _window.GetObservable(Visual.IsVisibleProperty)
           .Where(b => b)
           .ToUnit();

    public override IObservable<Unit> Unloaded
        => _window.GetObservable(Visual.IsVisibleProperty)
           .Where(b => !b)
           .ToUnit();
}