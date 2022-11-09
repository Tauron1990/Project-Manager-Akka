using System;
using Akka.Hosting;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using Servicemnager.Networking.IPC;
using Tauron.AkkaHost;
using Tauron.Application.Master.Commands.KillSwitch;

// ReSharper disable once CheckNamespace
namespace Tauron.Application.AkkaNode.Bootstrap;

public static partial class Bootstrap
{
    private const string IpcNameStr = "Project_Manager_{{A9A782E5-4F9A-46E4-8A71-76BCF1ABA748}}";
    internal static readonly SharmProcessId IpcName = SharmProcessId.From(IpcNameStr);

    [PublicAPI]
    public static IHostBuilder StartNode(string[] args, KillRecpientType type, IpcApplicationType ipcType, Action<IActorApplicationBuilder>? build = null, bool consoleLog = false)
        => StartNode(Host.CreateDefaultBuilder(args), type, ipcType, build, consoleLog);

    [PublicAPI]
    public static IHostBuilder StartNode(this IHostBuilder builder, KillRecpientType type, IpcApplicationType ipcType, Action<IActorApplicationBuilder>? build = null, bool consoleLog = false)
    {
        var masterReady = false;
        if(ipcType != IpcApplicationType.NoIpc)
            masterReady = SharmComunicator.MasterIpcReady(IpcName);
        var ipc = new IpcConnection(
            masterReady,
            ipcType,
            (s, exception) => LogManager.GetCurrentClassLogger().Error(exception, "Ipc Error: {Info}", s));

        void ConfigLogging(HostBuilderContext context, ILoggingBuilder configuration)
        {
            Console.Title = context.HostingEnvironment.ApplicationName;
            if(consoleLog)
                configuration.AddConsole();
        }

        void ConfigNode(IActorApplicationBuilder ab)
        {
            void ConfigurateServices(HostBuilderContext host, IServiceCollection cb)
            {
                cb.TryAddSingleton(host.Configuration);

                #pragma warning disable GU0011
                cb.AddHostedService<NodeAppService>()
                   .AddSingleton<KillHelper>()
                   .AddSingleton<IIpcConnection>(ipc);
            }

            void ConfigurateAkka(HostBuilderContext _, IServiceProvider provider, AkkaConfigurationBuilder systemBuilder)
            {
                systemBuilder.AddStartup(
                    (system, _) =>
                    {
                        provider.GetRequiredService<KillHelper>().Run();

                        switch (type)
                        {
                            case KillRecpientType.Seed:
                                KillSwitch.Setup(system);

                                break;
                            default:
                                KillSwitch.Subscribe(system, type);

                                break;
                        }
                    });
            }

            ab.ConfigureServices(ConfigurateServices)
               .ConfigureAkka(ConfigurateAkka);

            build?.Invoke(ab);
        }

        return builder
           .ConfigureLogging(ConfigLogging)
           .ConfigurateNode(ConfigNode);
    }
}