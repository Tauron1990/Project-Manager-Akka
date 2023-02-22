using System;
using Microsoft.Extensions.DependencyInjection;
using SimpleProjectManager.Client.Shared.Data.Files;
using Tauron;
using Tauron.Applicarion.Redux;

namespace SimpleProjectManager.Client.Shared.Data;

#pragma warning disable GU0011

public static class DataModule
{
    public static void Load(IServiceCollection collection)
    {
        new CommonModule().Load(collection);

        collection.AddTransient<UploadTransaction>();
        collection.AddScoped<Func<UploadTransaction>>(s => s.GetRequiredService<UploadTransaction>);
        collection.AddSingleton<GlobalState>();

        collection.AddStoreConfiguration();
    }
}