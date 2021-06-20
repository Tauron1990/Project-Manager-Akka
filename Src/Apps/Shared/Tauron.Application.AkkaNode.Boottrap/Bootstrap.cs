using System;
using System.IO;
using System.Reflection;
using Akka.Actor;
using Akka.Cluster;
using Akka.Configuration;
using Autofac;
using Autofac.Builder;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Tauron.Application.AkkaNode.Services.Configuration;
using Tauron.Application.Logging;
using Tauron.Application.Master.Commands.Administration.Configuration;
using Tauron.Host;

namespace Tauron.Application.AkkaNode.Bootstrap
{
    public static partial class Bootstrap
    {
        [PublicAPI]
        public static IActorApplicationBuilder ConfigurateNode(this IActorApplicationBuilder builder)
        {
            var config = GetConfig();

            return builder
                  .ConfigureAutoFac(cb => cb.Register(context =>
                                                      {
                                                          var opt = new AppNodeInfo();
                                                          context.Resolve<IConfiguration>().Bind(opt);
                                                          return opt;
                                                      }))
                  .Configuration(configurationBuilder => configurationBuilder.Add(
                                     new HoconConfigurationSource(() => config,
                                         ("akka.appinfo.applicationName", "ApplicationName"),
                                         ("akka.appinfo.actorsystem", "Actorsystem"),
                                         ("akka.appinfo.appslocation", "AppsLocation"))))
                  .ConfigureAkka(_ => config)
                  .ConfigureLogging((context, configuration) => configuration.ConfigDefaultLogging(context.HostEnvironment.ApplicationName));
        }

        [PublicAPI]
        public static IRegistrationBuilder<TImpl, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterStartUpAction<TImpl>(this ContainerBuilder builder)
            where TImpl : IStartUpAction
            => builder.RegisterType<TImpl>().As<IStartUpAction>();

        public static IActorApplicationBuilder OnMemberUp(this IActorApplicationBuilder builder, Action<HostBuilderContext, ActorSystem, Cluster> up)
        {
            return builder.ConfigurateAkkaSystem((context, system) =>
                                                 {
                                                     var cluster = Cluster.Get(system);
                                                     cluster.RegisterOnMemberUp(() => up(context, system, cluster));
                                                 });
        }

        public static IActorApplicationBuilder OnMemberRemoved(this IActorApplicationBuilder builder, Action<HostBuilderContext, ActorSystem, Cluster> remove)
        {
            return builder.ConfigurateAkkaSystem((context, system) =>
                                                 {
                                                     var cluster = Cluster.Get(system);
                                                     cluster.RegisterOnMemberRemoved(() => remove(context, system, cluster));
                                                 });
        }

        private static Config GetConfig()
        {
            var config = Config.Empty;

            var basePath = Assembly.GetEntryAssembly()?.Location ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(basePath))
                basePath = Path.GetDirectoryName(basePath) ?? string.Empty;

            if (File.Exists(Path.Combine(basePath, AkkaConfigurationBuilder.Base)))
                config = ConfigurationFactory.ParseString(File.ReadAllText(Path.Combine(basePath, AkkaConfigurationBuilder.Base))).WithFallback(config);

            if (File.Exists(Path.Combine(basePath, AkkaConfigurationBuilder.Main)))
                config = ConfigurationFactory.ParseString(File.ReadAllText(Path.Combine(basePath, AkkaConfigurationBuilder.Main))).WithFallback(config);

            if (File.Exists(Path.Combine(basePath, AkkaConfigurationBuilder.Seed)))
                config = ConfigurationFactory.ParseString(File.ReadAllText(Path.Combine(basePath, AkkaConfigurationBuilder.Seed))).WithFallback(config);

            return config;
        }
    }
}