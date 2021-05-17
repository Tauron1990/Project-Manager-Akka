using System.Reactive.Linq;
using System.Reactive.Subjects;
using ServiceHost.Client.Shared.ConfigurationServer.Data;
using ServiceHost.Client.Shared.ConfigurationServer.Events;
using Tauron.Features;

namespace ServiceManager.ServiceDeamon.ConfigurationServer.Internal
{
    public sealed class HostMonitor : ActorFeatureBase<HostMonitor.State>
    {
        public record State(string Name, ISubject<IConfigEvent> Publisher, ServerConfigugration ServerConfigugration);

        public static IPreparedFeature New(string name, ISubject<IConfigEvent> publisher, ServerConfigugration serverConfigugration)
            => Feature.Create(() => new HostMonitor(), new State(name, publisher, serverConfigugration));

        protected override void ConfigImpl()
        {
            CurrentState.Publisher.OfType<GlobalConfigEvent>()
        }
    }
}