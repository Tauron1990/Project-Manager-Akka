using System.Reactive.Disposables;
using SimpleProjectManager.Client.Data.Cache;
using SimpleProjectManager.Client.Data.Core;
using Tauron;
using Tavenem.Blazor.IndexedDB;

namespace SimpleProjectManager.Client.Data;

public class DataModule : IModule
{
    public void Load(IServiceCollection collection)
    {
        collection.RegisterModules(new CommonModule(), new DataModule());
        
        collection.AddIndexedDb(new IndexedDb<CacheDataId>(nameof(CacheData)));
        collection.AddIndexedDb(new IndexedDb<CacheTimeoutId>(nameof(CacheTimeout)));

        collection.AddSingleton<GlobalState>();
        collection.AddSingleton<CompositeDisposable>();
        
        collection.AddScoped<CacheDb>();
        collection.AddScoped<TimeoutManager>();
        collection.AddScoped<StateDb>();

        collection.AddTransient<IStoreConfiguration, StoreConfiguration>();
    }
}