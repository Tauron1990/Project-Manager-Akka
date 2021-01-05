using System;
using Autofac;
using Autofac.Builder;
using JetBrains.Annotations;
using Tauron.Application.CommonUI;
using Tauron.Application.CommonUI.AppCore;
using Tauron.Application.Wpf;
using Tauron.Application.Wpf.AppCore;
using Window = System.Windows.Window;

// ReSharper disable once CheckNamespace
namespace Tauron.Host
{
    [PublicAPI]
    public static class WpfHostExtensions
    {
        public static IApplicationBuilder UseWpf<TMainWindow>(this IApplicationBuilder hostBuilder, Action<BaseAppConfiguration>? config = null)
            where TMainWindow : class, IMainWindow
        {
            hostBuilder.ConfigureAutoFac(sc =>
            {
                sc.RegisterModule<WpfModule>();

                sc.RegisterType<TMainWindow>().As<IMainWindow>().SingleInstance();

                var wpf = new BaseAppConfiguration(sc);
                config?.Invoke(wpf);
            });

            return hostBuilder;
        }

        public static IApplicationBuilder UseWpf<TMainWindow, TApp>(this IApplicationBuilder builder) 
            where TApp : System.Windows.Application, new() 
            where TMainWindow : class, IMainWindow
        {
            return UseWpf<TMainWindow>(builder, c => c.WithAppFactory(() => new WpfFramework.DelegateApplication(new TApp())));
        }

        public static IRegistrationBuilder<SimpleSplashScreen<TWindow>, ConcreteReflectionActivatorData, SingleRegistrationStyle>
            AddSplash<TWindow>(this ContainerBuilder collection) where TWindow : Window, IWindow, new() => collection.RegisterType<SimpleSplashScreen<TWindow>>().As<ISplashScreen>();
    }
}