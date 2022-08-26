using System.Collections.Immutable;
using LiteDB;
using LiteDB.Async;
using Tauron.AkkaHost;

namespace SimpleProjectManager.Server.Data.LiteDbDriver;

public class LiteDbDriverModule : IDatabaseModule
{
    public void Configurate(IActorApplicationBuilder builder, ImmutableDictionary<string, string> propertys)
    {
    }
}