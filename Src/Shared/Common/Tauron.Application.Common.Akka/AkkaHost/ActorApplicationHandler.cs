using Microsoft.Extensions.DependencyInjection;
using Tauron.Module.Internal;

namespace Tauron.AkkaHost;

internal sealed class ActorApplicationHandler : IModuleHandler
{
    private readonly IActorApplicationBuilder _builder;

    internal ActorApplicationHandler(IActorApplicationBuilder builder) => _builder = builder;

    public void Handle(IServiceCollection collection, IModule module)
    {
        module.Load(collection);
        
        if(module is AkkaModule akkaModule)
            akkaModule.Load(_builder);
    }
}