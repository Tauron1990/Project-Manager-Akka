using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using ReactiveUI;
using SimpleProjectManager.Client.Avalonia.ViewModels.AppBar;

namespace SimpleProjectManager.Client.Avalonia.Views.AppBar;

public partial class MainAppBar : ReactiveUserControl<AppBarViewModel>
{
    public MainAppBar()
    {
        this.WhenActivated(Init);

        InitializeComponent();

        IEnumerable<IDisposable> Init()
        {
            yield return this.OneWayBind(ViewModel, m => m.ErrorModel, v => v.ErrorNotify.Content);
            yield return this.OneWayBind(ViewModel, m => m.ClockModel, v => v.ClockControl.Content);
        }
    }

    // ReSharper disable UnusedParameter.Local
    private void Exit_OnClick(object? sender, RoutedEventArgs e)
        // ReSharper restore UnusedParameter.Local
    {
        IApplicationLifetime? lifetime = Application.Current?.ApplicationLifetime;

        switch (lifetime)
        {
            case IClassicDesktopStyleApplicationLifetime app:
                app.Shutdown();

                break;
            default:
                Environment.Exit(-1);

                break;
        }
    }
}