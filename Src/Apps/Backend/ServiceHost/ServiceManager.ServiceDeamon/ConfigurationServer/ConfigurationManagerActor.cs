using Akka.Actor;
using SharpRepository.Repository.Configuration;
using Tauron.Features;

namespace ServiceManager.ServiceDeamon.ConfigurationServer
{
    public sealed class ConfigurationManagerActor : ActorFeatureBase<EmptyState>
    {
        public static Props New(ISharpRepositoryConfiguration configuration)
            => Feature.Props(Feature.Create(() => new ConfigurationManagerActor()));
    }
}