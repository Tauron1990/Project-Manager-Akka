﻿using Akka.Cluster.Hosting;
using Akka.Cluster.Hosting.SBR;
using Akka.Configuration;
using Akka.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SimpleProjectManager.Operation.Client.Config;
using SimpleProjectManager.Operation.Client.Core;
using SimpleProjectManager.Operation.Client.Device;
using SimpleProjectManager.Operation.Client.ImageEditor;
using SimpleProjectManager.Operation.Client.Setup;
using SimpleProjectManager.Shared.ServerApi;
using Tauron.AkkaHost;
using Tauron.Application.AkkaNode.Bootstrap;

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

    public async ValueTask<IHost> CreateClient(string[] args)
    {
        IConfigurationRoot setupConfig = new ConfigurationBuilder().AddCommandLine(args).Build();
        var setup = new SetupRunner(_configManager, _clientInteraction);
        await setup.RunSetup(setupConfig).ConfigureAwait(false);

        return Host.CreateDefaultBuilder(args)
           .ConfigureAppConfiguration((_, b) => b.AddInMemoryCollection(new[] { KeyValuePair.Create("actorsystem", "SimpleProjectManager-Server") }!))
           .ConfigureServices(ApplyFusionServices)
           .ConfigurateNode(ApplyClientServices)
           .Build();
    }

    private void ApplyFusionServices(IServiceCollection collection)
        => ClientRegistration.ConfigFusion(collection, new Uri(_configManager.Configuration.ServerUrl));

    private void ApplyClientServices(IActorApplicationBuilder actorApplication)
    {
        actorApplication
           .ConfigureAkka(
                (_, b) => b
                   .WithClustering(new ClusterOptions{ Roles = new []{"ProjectManager"}}, SplitBrainResolverOption.Default)
                   .WithDistributedPubSub("ProjectManager"))
           .ConfigureServices(
                (_, coll) => coll
                   .AddSingleton(_configManager.Configuration)
                   .AddSingleton<HostStarter>()
                   .AddSingleton<IHostedService>(sp => sp.GetRequiredService<HostStarter>()))
           .RegisterStartUp<ImageEditorStartup>(e => e.Run())
           .RegisterStartUp<DeviceStartUp>(d => d.Run());
    }
}