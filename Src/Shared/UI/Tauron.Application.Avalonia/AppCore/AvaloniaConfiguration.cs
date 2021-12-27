using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Microsoft.Extensions.DependencyInjection;
using Tauron.Application.CommonUI.AppCore;

namespace Tauron.Application.Avalonia.AppCore
{
    public sealed class AvaloniaConfiguration : BaseAppConfiguration
    {
        public AvaloniaConfiguration(IServiceCollection serviceCollection)
            : base(serviceCollection) { }

        public AvaloniaConfiguration WithApp<TApp>(Func<AppBuilder, AppBuilder> config)
            where TApp : global::Avalonia.Application, new()
        {
            var appBuilder = AppBuilder.Configure<TApp>();
            appBuilder = config(appBuilder).SetupWithLifetime(new ClassicDesktopStyleApplicationLifetime());

            WithAppFactory(() => new AvaloniaFramework.DelegateAvalonApp(appBuilder.Instance));

            return this;
        }
    }
}