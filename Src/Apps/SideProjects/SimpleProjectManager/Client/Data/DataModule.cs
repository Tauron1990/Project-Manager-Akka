using SimpleProjectManager.Client.Data.Cache;
using Tauron;
using Tavenem.Blazor.IndexedDB;

namespace SimpleProjectManager.Client.Data;

public class DataModule : IModule
{
    public void Load(IServiceCollection collection)
    {
        collection.RegisterModules(new CommonModule(), new DataModule());
        
        collection.AddIndexedDb(new IndexedDb<string>(nameof(CacheData)));
        collection.AddIndexedDb(new IndexedDb<int>(nameof(CacheTimeout)));
        
        collection.AddScoped<CacheDb>();
        collection.AddSingleton<TimeoutManager>();
    }
}