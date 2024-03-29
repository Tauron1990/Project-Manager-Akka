﻿using System.Reactive.Disposables;
using MudBlazor;
using Tauron.Application;
using Tauron.Application.Blazor;

namespace SimpleProjectManager.Client.Shared;

public partial class MainLayout
{
    private readonly MudTheme _darkTheme =
        new()
        {
            Palette = new Palette
                      {
                          Primary = "#776be7",
                          Black = "#27272f",
                          Background = "#32333d",
                          BackgroundGrey = "#27272f",
                          Surface = "#373740",
                          DrawerBackground = "#27272f",
                          DrawerText = "rgba(255,255,255, 0.50)",
                          DrawerIcon = "rgba(255,255,255, 0.50)",
                          AppbarBackground = "#27272f",
                          AppbarText = "rgba(255,255,255, 0.70)",
                          TextPrimary = "rgba(255,255,255, 0.70)",
                          TextSecondary = "rgba(255,255,255, 0.50)",
                          ActionDefault = "#adadb1",
                          ActionDisabled = "rgba(255,255,255, 0.26)",
                          ActionDisabledBackground = "rgba(255,255,255, 0.12)",
                          Divider = "rgba(255,255,255, 0.12)",
                          DividerLight = "rgba(255,255,255, 0.06)",
                          TableLines = "rgba(255,255,255, 0.12)",
                          LinesDefault = "rgba(255,255,255, 0.12)",
                          LinesInputs = "rgba(255,255,255, 0.3)",
                          TextDisabled = "rgba(255,255,255, 0.2)",
                          Info = "#3299ff",
                          Success = "#0bba83",
                          Warning = "#ffa800",
                          Error = "#f64e62",
                          Dark = "#27272f",
                      },
        };

    private bool _open;

    private IDisposable _subscription = Disposable.Empty;

    public void Dispose()
        => _subscription.Dispose();

    private void ToggleDrawer() => _open = !_open;

    protected override void OnInitialized()
    {
        _subscription = _aggregator.GetEvent<AggregateEvent<SnackbarMessage>, SnackbarMessage>().Get().Subscribe(m => m.Apply(_snackbar));
        base.OnInitialized();
    }


    private void OnNavigating()
        => _open = false;
}