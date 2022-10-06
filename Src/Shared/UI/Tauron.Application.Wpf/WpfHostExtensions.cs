using System;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Tauron.Application.CommonUI;
using Tauron.Application.CommonUI.AppCore;
using Tauron.Application.Wpf;
using Tauron.Application.Wpf.AppCore;
using Window = System.Windows.Window;
#pragma warning disable GU0011

// ReSharper disable once CheckNamespace
namespace Tauron.AkkaHost;

[PublicAPI]
public static class WpfHostExtensions
{
    public static IActorApplicationBuilder UseWpf<TMainWindow>(this IActorApplicationBuilder hostBuilder, Action<BaseAppConfiguration>? config = null)
        where TMainWindow : class, IMainWindow
    {
        hostBuilder.ConfigureServices(
            (_, sc) =>
            {
                sc.AddSingleton<IMainWindow, TMainWindow>();

                var wpf = new BaseAppConfiguration(sc);
                config?.Invoke(wpf);
            });

        return hostBuilder;
    }

    public static IActorApplicationBuilder UseWpf<TMainWindow, TApp>(this IActorApplicationBuilder builder)
        where TApp : System.Windows.Application, new()
        where TMainWindow : class, IMainWindow
    {
        return UseWpf<TMainWindow>(
            builder,
            c => c.WithAppFactory(() => new WpfFramework.DelegateApplication(new TApp())));
    }

    public static IServiceCollection AddSplash<TWindow>(this IServiceCollection collection) where TWindow : Window, IWindow, new()
        => collection.AddScoped<ISplashScreen, SimpleSplashScreen<TWindow>>();
}