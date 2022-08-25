using System.Collections.Immutable;
using Tauron.AkkaHost;

namespace SimpleProjectManager.Server.Data;

public interface IDatabaseModule
{
    void Configurate(IActorApplicationBuilder builder, ImmutableDictionary<string, string> propertys);
}