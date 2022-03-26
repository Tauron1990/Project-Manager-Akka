using SimpleProjectManager.Client.Shared.Data;
using SimpleProjectManager.Client.Shared.Services;
using Tauron;
using Tauron.Applicarion.Redux.Extensions.Cache;

namespace SimpleProjectManager.Client.Data;

public class InternalDataModule : IModule
{
    public void Load(IServiceCollection collection)
    {
        collection.RegisterModule<DataModule>();
        
        collection.AddScoped<ICacheDb, CacheDb>();
        collection.AddSingleton<IOnlineMonitor, OnlineMonitor>();
        collection.AddTransient<IMessageMapper, MessageMapper>();
    }
}