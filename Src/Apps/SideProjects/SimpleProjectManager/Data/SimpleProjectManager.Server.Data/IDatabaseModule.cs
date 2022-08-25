using Tauron.AkkaHost;

namespace SimpleProjectManager.Server.Data;

public interface IDatabaseModule
{
    void Configurate(IActorApplicationBuilder builder);
}