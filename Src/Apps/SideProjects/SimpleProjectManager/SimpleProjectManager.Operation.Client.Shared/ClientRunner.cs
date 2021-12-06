using Autofac;
using Microsoft.Extensions.DependencyInjection;
using SimpleProjectManager.Operation.Client.Shared.Config;
using SimpleProjectManager.Operation.Client.Shared.ImageEditor;
using SimpleProjectManager.Shared.ServerApi;
using Tauron.Application.AkkaNode.Bootstrap;

namespace SimpleProjectManager.Operation.Client.Shared;

public sealed class ClientRunner
{
    private ConfigManager ConfigManager { get; } = new();
    
    public void ApplyFusionServices(IServiceCollection collection)
        => ClientRegistration.ConfigFusion(collection, new Uri(ConfigManager.Configuration.ServerIp));

    public void ApplyClientServices(ContainerBuilder collection)
    {
        collection.RegisterStartUpAction<ImageEditorStartup>();
        
        collection.RegisterInstance(this);
        collection.RegisterInstance(this);
    }
}