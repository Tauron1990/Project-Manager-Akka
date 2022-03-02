using SimpleProjectManager.Client.Data.Core;
using SimpleProjectManager.Client.ViewModels;
using Tauron;
using Tauron.Applicarion.Redux;
using Tauron.Applicarion.Redux.Configuration;
using Tauron.Applicarion.Redux.Extensions.Cache;

namespace SimpleProjectManager.Client.Data;

public class DataModule : IModule
{
    public void Load(IServiceCollection collection)
    {
        collection.RegisterModules(new CommonModule());

        collection.AddTransient<UploadTransaction>();
        collection.AddScoped<Func<UploadTransaction>>(s => s.GetRequiredService<UploadTransaction>);
        collection.AddSingleton<GlobalState>();
        
        collection.AddScoped<IOnlineMonitor, OnlineMonitor>();
        collection.AddScoped<ICacheDb, CacheDb>();
        collection.AddScoped<IErrorHandler, ErrorHandler>();
        collection.AddStoreConfiguration();
    }
}