using System.Collections.Immutable;
using LiteDB;
using Microsoft.Extensions.DependencyInjection;
using Tauron.AkkaHost;

namespace SimpleProjectManager.Server.Data.LiteDbDriver;

public class LiteDbDriverModule : IDatabaseModule
{
    public void Configurate(IActorApplicationBuilder builder, ImmutableDictionary<string, string> propertys)
    {
        SerializationHelper.Init();

        var connectionString = propertys["LiteConnection"];

        builder.ConfigureServices(
            (_, services) =>
            {
                services.AddSingleton<ILiteDatabase>(
                    string.IsNullOrEmpty(connectionString)
                        ? new LiteDatabase(new MemoryStream())
                        : new LiteDatabase(connectionString));
                
                services.AddSingleton<IInternalFileRepository, LiteFileRepository>();
                services.AddSingleton<IInternalDataRepository, LiteDataRepository>();
            });
    }
}