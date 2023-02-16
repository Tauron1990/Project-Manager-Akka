using Microsoft.Extensions.DependencyInjection;

namespace Tauron.AkkaHost;

public abstract class AkkaModule : IModule
{
    public virtual void Load(IActorApplicationBuilder builder)
    {
        
    }
    
    public virtual void Load(IServiceCollection collection)
    {
        
    }
}