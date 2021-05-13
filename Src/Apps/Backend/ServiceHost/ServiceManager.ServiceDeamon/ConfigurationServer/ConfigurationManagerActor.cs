using System;
using System.Reactive.Linq;
using Akka.Actor;
using ServiceHost.Client.Shared.ConfigurationServer;
using ServiceHost.Client.Shared.ConfigurationServer.Data;
using ServiceManager.ServiceDeamon.ConfigurationServer.Data;
using ServiceManager.ServiceDeamon.ConfigurationServer.Internal;
using SharpRepository.Repository;
using SharpRepository.Repository.Configuration;
using Tauron;
using Tauron.Akka;
using Tauron.Features;

namespace ServiceManager.ServiceDeamon.ConfigurationServer
{
    public sealed class ConfigurationManagerActor : ActorFeatureBase<ConfigurationManagerActor.State>
    {
        private const string ServerConfigurationId = "86FCF2E7-10D0-4103-82F4-D0F0E08995EF";

        public sealed record State(Func<ConfigFeatureConfiguration> Factory, IRepository<ServerConfigurationEntity, string> ServerConfiguration);

        public static Props New(ISharpRepositoryConfiguration repository)
        {
            var serverConfiguration = repository.GetInstance<ServerConfigurationEntity, string>();

            ConfigFeatureConfiguration CreateState()
                => new(serverConfiguration.Get(ServerConfigurationId)?.Configugration ?? new ServerConfigugration(true, false, true),
                    repository.GetInstance<GlobalConfigEntity, string>(),
                    repository.GetInstance<SeedUrlEntity, string>(),
                    repository.GetInstance<SpecificConfigEntity, string>());

            return Feature.Props(Feature.Create(() => new ConfigurationManagerActor(), _ => new State(CreateState, serverConfiguration)));
        }

        protected override void ConfigImpl()
        {
            var querys = Context.ActorOf("Querys", Feature.Create(() => new ConfigQueryFeature(), _ => CurrentState.Factory()));
            var commands = Context.ActorOf("Commands", Feature.Create(() => new ConfigCommandFeature(), _ => CurrentState.Factory()));

            Receive<IConfigQuery>(obs => obs.Select(o => o.Event)
                                            .ToActor(querys));
            Receive<IConfigCommand>(obs => obs.Select(c => c.Event)
                                              .ToActor(commands));
        }
    }
}