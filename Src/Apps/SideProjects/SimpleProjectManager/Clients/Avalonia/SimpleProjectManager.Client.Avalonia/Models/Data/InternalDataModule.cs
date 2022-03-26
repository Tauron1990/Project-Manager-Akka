using Microsoft.Extensions.DependencyInjection;
using SimpleProjectManager.Client.Shared.Data;
using SimpleProjectManager.Client.Shared.Services;
using Tauron;
using Tauron.Applicarion.Redux.Configuration;
using Tauron.Applicarion.Redux.Extensions.Cache;

namespace SimpleProjectManager.Client.Avalonia.Models.Data;

public sealed class InternalDataModule : IModule
{
    public void Load(IServiceCollection collection)
    {
        collection.RegisterModule<DataModule>();

        collection.AddSingleton<IErrorHandler, ErrorHandler>();
        collection.AddSingleton<ICacheDb, LocalCacheDb>();
        collection.AddSingleton<IOnlineMonitor, OnlineMonitor>();
        collection.AddTransient<IMessageMapper, MessageMapper>();
        
        collection.AddTransient<LocalCacheDbContext>();
    }
}