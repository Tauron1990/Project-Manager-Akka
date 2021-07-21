using System;
using Autofac;
using Autofac.Builder;
using Avalonia;
using JetBrains.Annotations;
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
        public static IActorApplicationBuilder UseAvalonia<TMainWindow>(this IActorApplicationBuilder hostBuilder,
            Action<AvaloniaConfiguration>? config = null)
            where TMainWindow : class, IMainWindow
        {
            hostBuilder.ConfigureAutoFac(sc =>
            {
                sc.RegisterModule<AvaloniaModule>();

                sc.RegisterType<TMainWindow>().As<IMainWindow>().SingleInstance();

                var avaloniaConfiguration = new AvaloniaConfiguration(sc);
                config?.Invoke(avaloniaConfiguration);
            });

            return hostBuilder;
        }

        public static IActorApplicationBuilder UseAvalonia<TMainWindow, TApp>(this IActorApplicationBuilder builder,
            Func<AppBuilder, AppBuilder> config)
            where TMainWindow : class, IMainWindow
            where TApp : Avalonia.Application, new()
        {
            return UseAvalonia<TMainWindow>(builder, c => c.WithApp<TApp>(config));
        }

        public static IRegistrationBuilder<SimpleSplashScreen<TWindow>, ConcreteReflectionActivatorData,
                SingleRegistrationStyle>
            AddSplash<TWindow>(this ContainerBuilder collection) where TWindow : Window, IWindow, new()
            => collection.RegisterType<SimpleSplashScreen<TWindow>>().As<ISplashScreen>();
    }
}