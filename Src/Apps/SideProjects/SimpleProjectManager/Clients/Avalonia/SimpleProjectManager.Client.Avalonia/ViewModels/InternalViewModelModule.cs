using Microsoft.Extensions.DependencyInjection;
using SimpleProjectManager.Client.Avalonia.ViewModels.AppBar;
using Tauron;

namespace SimpleProjectManager.Client.Avalonia.ViewModels;

public sealed class InternalViewModelModule : IModule
{
    public void Load(IServiceCollection collection)
    {
        collection.AddTransient<MainWindowViewModel>();

        collection.AddTransient<AppBarViewModel>();
        collection.AddTransient<NotifyErrorModel>();
        collection.AddTransient<ClockDisplayViewModel>();
    }
}