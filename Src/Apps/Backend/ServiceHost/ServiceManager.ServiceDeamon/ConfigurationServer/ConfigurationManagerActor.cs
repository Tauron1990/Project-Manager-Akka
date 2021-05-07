using System;
using Akka.Actor;
using ServiceManager.ServiceDeamon.ConfigurationServer.Internal;
using SharpRepository.Repository.Configuration;
using Tauron.Akka;
using Tauron.Features;

namespace ServiceManager.ServiceDeamon.ConfigurationServer
{
    public sealed class ConfigurationManagerActor : ActorFeatureBase<ConfigurationManagerActor.State>
    {
        public sealed record State(Func<ConfigFeatureConfiguration> Factory);

        public static Props New(ISharpRepositoryConfiguration repository)
        {

        }

        protected override void ConfigImpl()
        {
            
        }
    }
}