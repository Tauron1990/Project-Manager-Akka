using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Tauron.Application.CommonUI.AppCore;

namespace Tauron.Application.Avalonia.AppCore;

[PublicAPI]
public sealed class AvaloniaConfiguration : BaseAppConfiguration
{
    public AvaloniaConfiguration(IServiceCollection serviceCollection)
        : base(serviceCollection) { }

    public AvaloniaConfiguration WithApp<TApp>(Func<AppBuilder, AppBuilder> config)
        where TApp : global::Avalonia.Application, new()
    {
        AppBuilder? appBuilder = AppBuilder.Configure<TApp>();
        appBuilder = config(appBuilder).SetupWithLifetime(new ClassicDesktopStyleApplicationLifetime());

        WithAppFactory(() => new AvaloniaFramework.DelegateAvalonApp(appBuilder.Instance));

        return this;
    }
}