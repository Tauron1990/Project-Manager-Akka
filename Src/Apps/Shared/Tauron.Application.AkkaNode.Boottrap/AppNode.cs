using System;
using System.IO;
using System.Reflection;
using Akka.Actor;
using Akka.Cluster;
using Akka.Cluster.Hosting;
using Akka.Cluster.Utility;
using Akka.Configuration;
using Akka.DistributedData;
using Akka.Hosting;
using Akka.Logger.NLog;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Tauron.AkkaHost;
using Tauron.Application.Logging;
using Tauron.Application.Master.Commands.Administration.Configuration;

namespace Tauron.Application.AkkaNode.Bootstrap;

[PublicAPI]
public static partial class AppNode
{
    [PublicAPI]
    public static IHostBuilder ConfigurateNode(this IHostBuilder builder, Action<IActorApplicationBuilder>? appConfig = null)
    {
        Config config = GetConfig();

        void ConfigurateAkka(HostBuilderContext _, AkkaConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.ConfigureLoggers(
                lcb =>
                {
                    lcb.ClearLoggers();
                    lcb.AddLogger<NLogLogger>();
                });

                var hyperion = 
                        """
akka {
  actor {
    serializers {
      hyperion = "Akka.Serialization.HyperionSerializer, Akka.Serialization.Hyperion"
    }
    serialization-bindings {
      "System.Object" = hyperion
    }
  }
}

""";
                
               configurationBuilder
                  .AddHocon(ConfigurationFactory.ParseString(hyperion), HoconAddMode.Replace)
               .WithExtension<DistributedDataProvider>()
               .WithExtension<ClusterActorDiscoveryId>()
               .WithClusterClientReceptionist()
               .AddHocon(config, HoconAddMode.Append);
        }

        void AkkaConfig(IActorApplicationBuilder ab)
        {
            ab.ConfigureServices((_, cb) => cb.AddSingleton(AppInfoFactory))
               .ConfigureAkka(ConfigurateAkka);

            appConfig?.Invoke(ab);
        }

        return builder
           .ConfigureAkkaApplication(AkkaConfig)
           .ConfigureLogging(ConfigureLogging);
    }

    private static void ConfigureLogging(HostBuilderContext context, ILoggingBuilder configuration)
    {
        configuration.ClearProviders();
        configuration.ConfigDefaultLogging(context.HostingEnvironment.ApplicationName);
    }

    private static AppNodeInfo AppInfoFactory(IServiceProvider context)
    {
        var opt = new AppNodeInfo();
        context.GetRequiredService<IConfiguration>().Bind(opt);

        return opt;
    }

    public static IActorApplicationBuilder OnMemberUp(this IActorApplicationBuilder builder, Action<HostBuilderContext, ActorSystem, Cluster> up)
    {
        return builder.ConfigureAkka(
            (context, systemBuilder) =>
            {
                systemBuilder.AddStartup(
                    (system, _) =>
                    {
                        Cluster? cluster = Cluster.Get(system);
                        cluster.RegisterOnMemberUp(() => up(context, system, cluster));
                    });
            });
    }

    public static IActorApplicationBuilder OnMemberRemoved(this IActorApplicationBuilder builder, Action<HostBuilderContext, ActorSystem, Cluster> remove)
    {
        return builder.ConfigureAkka(
            (context, systemBuilder) =>
            {
                systemBuilder.AddStartup(
                    (system, _) =>
                    {
                        Cluster? cluster = Cluster.Get(system);
                        cluster.RegisterOnMemberRemoved(() => remove(context, system, cluster));
                    });
            });
    }

    private static Config GetConfig()
    {
        var config = Config.Empty;

        string basePath = Assembly.GetEntryAssembly()?.Location ?? string.Empty;
        if(!string.IsNullOrWhiteSpace(basePath))
            basePath = Path.GetDirectoryName(basePath) ?? string.Empty;

        if(File.Exists(Path.Combine(basePath, AkkaConfigurationHelper.Main)))
            config = ConfigurationFactory.ParseString(File.ReadAllText(Path.Combine(basePath, AkkaConfigurationHelper.Main))).WithFallback(config);

        if(File.Exists(Path.Combine(basePath, AkkaConfigurationHelper.Seed)))
            config = ConfigurationFactory.ParseString(File.ReadAllText(Path.Combine(basePath, AkkaConfigurationHelper.Seed))).WithFallback(config);

        return config;
    }

    public static IActorApplicationBuilder RegisterStartUp<TStarter>(this IActorApplicationBuilder builder, Action<TStarter> onStart)
        where TStarter : class
        => builder
           .ConfigureServices((_, c) => c.AddSingleton<TStarter>())
           .ConfigureAkka((_, prov, b) => b.AddStartup((_, _) => onStart(prov.GetRequiredService<TStarter>())));
}