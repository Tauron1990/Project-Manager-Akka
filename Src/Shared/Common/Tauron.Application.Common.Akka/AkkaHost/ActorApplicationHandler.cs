using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tauron.Module.Internal;

namespace Tauron.AkkaHost;

internal sealed class ActorApplicationHandler : IModuleHandler
{
    private readonly IActorApplicationBuilder _builder;

    internal ActorApplicationHandler(IActorApplicationBuilder builder) => _builder = builder;

    public void Handle(IHostBuilder collection, IModule module)
    {
        collection.ConfigureServices(module.Load);
        
        if(module is AkkaModule akkaModule)
            akkaModule.Load(_builder);
    }
}