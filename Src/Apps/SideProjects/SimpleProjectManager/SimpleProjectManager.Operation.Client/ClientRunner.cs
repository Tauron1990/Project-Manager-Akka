using Akka.Cluster.Hosting;
using Akka.Remote.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SimpleProjectManager.Client.Operations.Shared;
using SimpleProjectManager.Operation.Client.Config;
using SimpleProjectManager.Operation.Client.Core;
using SimpleProjectManager.Operation.Client.Device;
using SimpleProjectManager.Operation.Client.Device.MGI.ProcessManager;
using SimpleProjectManager.Operation.Client.Device.MGI.UVLamp;
using SimpleProjectManager.Operation.Client.ImageEditor;
using SimpleProjectManager.Operation.Client.Setup;
using SimpleProjectManager.Shared.ServerApi;
using Tauron.AkkaHost;
using Tauron.Application.AkkaNode.Bootstrap;
#pragma warning disable GU0011
namespace SimpleProjectManager.Operation.Client;

public sealed class ClientRunner
{
    private readonly IClientInteraction _clientInteraction;
    private readonly ConfigManager _configManager;

    public ClientRunner(IClientInteraction clientInteraction)
    {
        _clientInteraction = clientInteraction;
        _configManager = new ConfigManager(clientInteraction);
    }

    public async ValueTask<IHost> CreateClient(ClientConfiguration configuration)
    {
        var setup = new SetupRunner(_configManager, _clientInteraction);
        await setup.RunSetup(configuration).ConfigureAwait(false);

        return Host.CreateDefaultBuilder(configuration.Args)
           .ConfigureHostConfiguration(b => b
                                         .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                                         .AddInMemoryCollection(new[]
                                                                {
                                                                    KeyValuePair.Create("actorsystem", "SimpleProjectManager-Server"),
                                                                }!)
                                      )
           .ConfigureServices((hb, sc) => sc.Configure<MgiOptions>("MGIConfig", hb.Configuration))
           .ConfigureServices(ApplyFusionServices)
           .ConfigurateNode(ApplyClientServices)
           .Build();
    }

    private void ApplyFusionServices(IServiceCollection collection)
        => ClientRegistration.ConfigFusion(collection, new Uri(_configManager.Configuration.ServerUrl));

    private void ApplyClientServices(IActorApplicationBuilder actorApplication)
    {
        actorApplication
            .ConfigureServices(
                (host, sc) =>
                {
                    sc.Configure<ProcessConfiguration>(host.Configuration.GetSection("MGI"));
                })
           .ConfigureAkka(
                (_, b) =>
                {
                    if(_configManager.Configuration.SelfPort.Value > 0)
                        b.WithRemoting(port: _configManager.Configuration.SelfPort.Value);

                    b
                       .WithClustering(new ClusterOptions { Roles = new[] { "ProjectManager" } })
                       .WithDistributedPubSub("ProjectManager");
                })
           .ConfigureServices(
                (_, coll) => coll
                   .AddSingleton<NameService>()
                   .AddSingleton(_configManager.Configuration)
                   .AddHostedService<HostStarter>()
                   .AddSingleton<HostStarter>())
           .RegisterStartUp<ImageEditorStartup>(e => e.Run())
           .RegisterStartUp<DeviceStartUp>(d => d.Run());
    }
}