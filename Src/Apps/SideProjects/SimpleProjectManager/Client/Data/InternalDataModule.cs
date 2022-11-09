using Microsoft.JSInterop;
using SimpleProjectManager.Client.Shared.Data;
using SimpleProjectManager.Client.Shared.Services;
using Tauron;
using Tauron.Applicarion.Redux;
using Tauron.Applicarion.Redux.Extensions.Cache;

namespace SimpleProjectManager.Client.Data;

public class InternalDataModule : IModule
{
    public void Load(IServiceCollection collection)
    {
        collection.RegisterModule<DataModule>();

        collection.AddScoped<ICacheDb, CacheDb>();
        collection.AddScoped<IOnlineMonitor, OnlineMonitor>();

        collection.AddTransient<IMessageDispatcher, MessageDispatcher>();
        collection.AddTransient<IErrorHandler, ErrorHandler>();
        collection.AddTransient<INavigationHelper, NavigationHelper>();
        collection.AddTransient<Func<IJSRuntime>>(provider => provider.GetRequiredService<IJSRuntime>);
    }
}