using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SimpleProjectManager.Operation.Client.Config;
using SimpleProjectManager.Operation.Client.Core;
using SimpleProjectManager.Operation.Client.ImageEditor;
using SimpleProjectManager.Operation.Client.Setup;
using SimpleProjectManager.Shared.ServerApi;
using Tauron.AkkaHost;
using Tauron.Application.AkkaNode.Bootstrap;

namespace SimpleProjectManager.Operation.Client;

public sealed class ClientRunner
{
    private readonly IClientInteraction _clientInteraction;
    private readonly ConfigManager _configManager = new();

    public ClientRunner(IClientInteraction clientInteraction)
        => _clientInteraction = clientInteraction;

    public async ValueTask<IHost> CreateClient(string[] args)
    {
        var setupConfig = new ConfigurationBuilder().AddCommandLine(args).Build();
        var setup = new SetupRunner(_configManager, _clientInteraction);
        await setup.RunSetup(setupConfig);

         return Host.CreateDefaultBuilder(args)
        
        .ConfigurateNode()
        .ConfigureAppConfiguration((_, b) => b.AddInMemoryCollection(new[] { KeyValuePair.Create("actorsystem", "SimpleProjectManager-Server") }))
        .ConfigureServices(ser => ser.AddSingleton(setup.Configuration))
        .ConfigureServices(ApplyFusionServices)
        .ConfigurateNode(ApplyClientServices)
        
        .Build();
    }

    private void ApplyFusionServices(IServiceCollection collection)
        => ClientRegistration.ConfigFusion(collection, new Uri(_configManager.Configuration.ServerUrl));

    private void ApplyClientServices(IActorApplicationBuilder actorApplication)
    {
        actorApplication
           .ConfigureServices((_, coll) => coll.AddSingleton(_configManager.Configuration))
           .RegisterStartUp<ImageEditorStartup>(e => e.Run());
    }
}