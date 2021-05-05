using System;
using Akka.Actor;
using Akka.Cluster.Utility;
using Akka.Event;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using ServiceHost.ApplicationRegistry;
using ServiceHost.Installer;
using ServiceHost.Services;
using Tauron.Application.Master.Commands.Administration.Host;
using static Tauron.Application.Master.Commands.Administration.Host.InternalHostMessages;

namespace ServiceHost.SharedApi
{
    [UsedImplicitly]
    public sealed class ApiDispatcherActor : ReceiveActor
    {
        public ApiDispatcherActor(IConfiguration configuration, Lazy<IAppManager> appManager, Lazy<IAppRegistry> appRegistry, Lazy<IInstaller> installer)
        {
            var hostName = configuration["applicationName"];

            Receive<GetHostName>(_ => Sender.Tell(new GetHostNameResult(hostName)));

            Receive<CommandBase>(cb =>
                                 {
                                     switch (cb.Type)
                                     {
                                         case CommandType.AppManager:
                                             appManager.Value.Actor.Forward(cb);
                                             break;
                                         case CommandType.AppRegistry:
                                             appRegistry.Value.Actor.Forward(cb);
                                             break;
                                         case CommandType.Installer:
                                             installer.Value.Actor.Forward(cb);
                                             break;
                                         default:
                                             Context.GetLogger().Warning("Unkowen Shared Api Command Sended {Type}", cb.GetType());
                                             break;
                                     }
                                 });
        }

        protected override void PreStart()
        {
            ClusterActorDiscovery.Get(Context.System)
               .Discovery.Tell(new ClusterActorDiscoveryMessage.RegisterActor(Self, HostApi.ApiKey));

            base.PreStart();
        }
    }
}