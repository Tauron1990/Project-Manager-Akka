using System;
using Avalonia;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Tauron.Application.Avalonia;
using Tauron.Application.Avalonia.AppCore;
using Tauron.Application.CommonUI;
using Tauron.Application.CommonUI.AppCore;

// ReSharper disable once CheckNamespace
namespace Tauron.AkkaHost
{
    [PublicAPI]
    public static class AvaloniaHostExtensions
    {
        public static IActorApplicationBuilder UseAvalonia<TMainWindow>(
            this IActorApplicationBuilder hostBuilder,
            Action<AvaloniaConfiguration>? config = null)
            where TMainWindow : class, IMainWindow
        {
            hostBuilder.ConfigureServices(
                (_, sc) =>
                {
                    sc.AddSingleton<IMainWindow, TMainWindow>();

                    var avaloniaConfiguration = new AvaloniaConfiguration(sc);
                    config?.Invoke(avaloniaConfiguration);
                });

            return hostBuilder;
        }

        public static IActorApplicationBuilder UseAvalonia<TMainWindow, TApp>(
            this IActorApplicationBuilder builder,
            Func<AppBuilder, AppBuilder> config)
            where TMainWindow : class, IMainWindow
            where TApp : Avalonia.Application, new()
        {
            return UseAvalonia<TMainWindow>(builder, c => c.WithApp<TApp>(config));
        }

        public static IServiceCollection AddSplash<TWindow>(this IServiceCollection collection) 
            where TWindow : Window, IWindow, new()
            => collection.AddTransient<ISplashScreen, SimpleSplashScreen<TWindow>>();
    }
}