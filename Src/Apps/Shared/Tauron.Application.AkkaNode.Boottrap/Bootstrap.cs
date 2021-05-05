using System.IO;
using Akka.Configuration;
using Autofac;
using Autofac.Builder;
using JetBrains.Annotations;
using Tauron.Application.AkkaNode.Services.Configuration;
using Tauron.Application.Logging;
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
                .ConfigureAppConfiguration((_, configurationBuilder) => configurationBuilder.Add(
                    new HoconConfigurationSource(() => config,
                        ("akka.appinfo.applicationName", "applicationName"),
                        ("akka.appinfo.actorsystem", "actorsystem"),
                        ("akka.appinfo.appslocation", "AppsLocation"))))
                .ConfigureAkka(_ => config)
                .ConfigureLogging((context, configuration) => configuration.ConfigDefaultLogging(context.HostEnvironment.ApplicationName));
        }

        [PublicAPI]
        public static IRegistrationBuilder<TImpl, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterStartUpAction<TImpl>(this ContainerBuilder builder)
            where TImpl : IStartUpAction
            => builder.RegisterType<TImpl>().As<IStartUpAction>();

        private static Config GetConfig()
        {
            const string baseconf = "base.conf";
            const string main = "akka.conf";
            const string seed = "seed.conf";

            var config = Config.Empty;

            if (File.Exists(baseconf))
                config = ConfigurationFactory.ParseString(File.ReadAllText(baseconf)).WithFallback(config);

            if (File.Exists(main))
                config = ConfigurationFactory.ParseString(File.ReadAllText(main)).WithFallback(config);

            if (File.Exists(seed))
                config = ConfigurationFactory.ParseString(File.ReadAllText(seed)).WithFallback(config);

            return config;
        }
    }
}