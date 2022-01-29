using System.Reactive.Disposables;
using SimpleProjectManager.Client.Data.Core;
using Tauron;
using Tauron.Applicarion.Redux;
using Tauron.Applicarion.Redux.Configuration;
using Tauron.Applicarion.Redux.Extensions.Cache;
using Tauron.Applicarion.Redux.Internal.Configuration;
using Tavenem.Blazor.IndexedDB;

namespace SimpleProjectManager.Client.Data;

public class DataModule : IModule
{
    public void Load(IServiceCollection collection)
    {
        collection.RegisterModules(new CommonModule());
        
        collection.AddIndexedDb(new IndexedDb<CacheDataId>(nameof(CacheData)));
        collection.AddIndexedDb(new IndexedDb<CacheTimeoutId>(nameof(CacheTimeout)));

        collection.AddSingleton<GlobalState>();
        
        collection.AddScoped<CacheDb>();
        collection.AddScoped<IErrorHandler, ErrorHandler>();
        collection.AddStoreConfiguration();
    }
}