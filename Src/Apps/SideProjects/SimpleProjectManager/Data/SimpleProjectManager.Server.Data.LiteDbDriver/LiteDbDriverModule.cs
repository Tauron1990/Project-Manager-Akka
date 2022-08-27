using System.Collections.Immutable;
using LiteDB;
using LiteDB.Async;
using Microsoft.Extensions.DependencyInjection;
using Tauron.AkkaHost;

namespace SimpleProjectManager.Server.Data.LiteDbDriver;

public class LiteDbDriverModule : IDatabaseModule
{
    public void Configurate(IActorApplicationBuilder builder, ImmutableDictionary<string, string> propertys)
    {
        var connectionString = propertys["LiteConnection"];

        builder.ConfigureServices(
            (_, services) =>
            {
                services.AddSingleton<ILiteDatabaseAsync>(
                    string.IsNullOrEmpty(connectionString)
                        ? new LiteDatabaseAsync(new LiteDatabase(new MemoryStream()))
                        : new LiteDatabaseAsync(new LiteDatabase(connectionString)));
                
                services.AddSingleton<IInternalFileRepository, LiteFileRepository>();
                services.AddSingleton<IInternalDataRepository, LiteDataRepository>();
            });
    }
}