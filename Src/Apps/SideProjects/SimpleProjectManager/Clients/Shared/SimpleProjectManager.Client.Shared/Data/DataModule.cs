using System;
using Microsoft.Extensions.DependencyInjection;
using SimpleProjectManager.Client.Shared.Data.Files;
using Tauron;
using Tauron.Applicarion.Redux;

namespace SimpleProjectManager.Client.Shared.Data;

public class DataModule : IModule
{
    public void Load(IServiceCollection collection)
    {
        collection.RegisterModules(new CommonModule());

        collection.AddTransient<UploadTransaction>();
        collection.AddScoped<Func<UploadTransaction>>(s => s.GetRequiredService<UploadTransaction>);
        collection.AddSingleton<GlobalState>();
        
        collection.AddStoreConfiguration();
    }
}