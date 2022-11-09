using System;
using Avalonia;
using Avalonia.ReactiveUI;

namespace SimpleProjectManager.Client.Avalonia;

public static class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        BuildAvaloniaApp()
           .StartWithClassicDesktopLifetime(args);

        App.Disposer.Dispose();
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    private static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
           .UsePlatformDetect()
           .LogToTrace()
           .UseReactiveUI();
}