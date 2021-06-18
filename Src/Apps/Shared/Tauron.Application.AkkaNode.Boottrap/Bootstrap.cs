using System;
using System.IO;
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
        public static IApplicationBuilder ConfigurateNode(this IApplicationBuilder builder)
        {
            var config = GetConfig();

            return builder
                  .ConfigureAutoFac(cb => cb.Register(context =>
                                                      {
                                                          var opt = new AppNodeInfo();
                                                          context.Resolve<IConfiguration>().Bind(opt);
                                                          return opt;
                                                      }))
                  .ConfigureAppConfiguration((_, configurationBuilder) => configurationBuilder.Add(
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

        public static IApplicationBuilder OnMemberUp(this IApplicationBuilder builder, Action<HostBuilderContext, ActorSystem, Cluster> up)
        {
            return builder.ConfigurateAkkaSystem((context, system) =>
                                                 {
                                                     var cluster = Cluster.Get(system);
                                                     cluster.RegisterOnMemberUp(() => up(context, system, cluster));
                                                 });
        }

        public static IApplicationBuilder OnMemberRemoved(this IApplicationBuilder builder, Action<HostBuilderContext, ActorSystem, Cluster> remove)
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

            if (File.Exists(AkkaConfigurationBuilder.Base))
                config = ConfigurationFactory.ParseString(File.ReadAllText(AkkaConfigurationBuilder.Base)).WithFallback(config);

            if (File.Exists(AkkaConfigurationBuilder.Main))
                config = ConfigurationFactory.ParseString(File.ReadAllText(AkkaConfigurationBuilder.Main)).WithFallback(config);

            if (File.Exists(AkkaConfigurationBuilder.Seed))
                config = ConfigurationFactory.ParseString(File.ReadAllText(AkkaConfigurationBuilder.Seed)).WithFallback(config);

            return config;
        }
    }
}