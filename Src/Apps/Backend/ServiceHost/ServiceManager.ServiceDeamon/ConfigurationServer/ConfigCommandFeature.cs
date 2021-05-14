using Akka.Actor;
using System.Reactive.Linq;
using ServiceHost.Client.Shared.ConfigurationServer;
using ServiceHost.Client.Shared.ConfigurationServer.Events;
using ServiceManager.ServiceDeamon.ConfigurationServer.Internal;
using Tauron;
using Tauron.Application.AkkaNode.Services.Reporting;
using Tauron.Features;
using Tauron.Operations;

namespace ServiceManager.ServiceDeamon.ConfigurationServer
{
    public class ConfigCommandFeature : ReportingActor<ConfigFeatureConfiguration>
    {
        protected override void ConfigImpl()
        {
            (from evt in CurrentState.EventPublisher
             where evt is ServerConfigurationEvent
             select ((ServerConfigurationEvent) evt).Configugration)
               .AutoSubscribe(s => Self.Tell(s)).DisposeWith(this);

            TryReceive<UpdateServerConfigurationCommand>(nameof(UpdateServerConfigurationCommand),
                obs => obs
                      .Select(m => m.Event.ServerConfigugration)
                      .UForwardToParent()
                      .Select(_ => OperationResult.Success()));
        }
    }
}