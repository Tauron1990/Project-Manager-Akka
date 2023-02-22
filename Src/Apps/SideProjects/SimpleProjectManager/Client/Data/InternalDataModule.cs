using Microsoft.JSInterop;
using SimpleProjectManager.Client.Shared.Data;
using SimpleProjectManager.Client.Shared.Services;
using Tauron;
using Tauron.Applicarion.Redux;
using Tauron.Applicarion.Redux.Extensions.Cache;

namespace SimpleProjectManager.Client.Data;

#pragma warning disable GU0011

public static class InternalDataModule
{
    public static void Load(IServiceCollection collection)
    {
        DataModule.Load(collection);

        collection.AddScoped<ICacheDb, CacheDb>();
        collection.AddScoped<IOnlineMonitor, OnlineMonitor>();

        collection.AddTransient<IMessageDispatcher, MessageDispatcher>();
        collection.AddTransient<IErrorHandler, ErrorHandler>();
        collection.AddTransient<INavigationHelper, NavigationHelper>();
        collection.AddTransient<Func<IJSRuntime>>(provider => provider.GetRequiredService<IJSRuntime>);
    }
}