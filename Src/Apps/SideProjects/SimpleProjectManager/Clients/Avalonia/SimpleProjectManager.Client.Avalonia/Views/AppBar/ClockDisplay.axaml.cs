using System;
using System.Collections.Generic;
using Avalonia.ReactiveUI;
using ReactiveUI;
using SimpleProjectManager.Client.Avalonia.ViewModels.AppBar;

namespace SimpleProjectManager.Client.Avalonia.Views.AppBar;

public partial class ClockDisplay : ReactiveUserControl<ClockDisplayViewModel>
{
    public ClockDisplay()
    {
        InitializeComponent();

        this.WhenActivated(Init);
    }

    private IEnumerable<IDisposable> Init()
    {
        yield return this.OneWayBind(ViewModel, m => m.CurrentTime, v => v.ClockValue.Text);
    }
}