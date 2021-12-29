using SimpleProjectManager.Client.Data.Cache;
using Stl.Fusion.Swapping;
using Tauron;
using Tauron.Application.Workshop;
using Tavenem.Blazor.IndexedDB;

namespace SimpleProjectManager.Client.Data;

public class DataModule : IModule
{
    public void Load(IServiceCollection collection)
    {
        collection.RegisterModules(new CommonModule(), new DataModule(), new TaskModule());
        
        collection.AddIndexedDb(new IndexedDb<string>(nameof(CacheData)));
        collection.AddIndexedDb(new IndexedDb<int>(nameof(CacheTimeout)));
        collection.AddScoped<CacheDb>();
        collection.AddScoped<TimeoutManager>();
        collection.AddSingleton<ISwapService, IndexedDdSwapService>();
    }
}