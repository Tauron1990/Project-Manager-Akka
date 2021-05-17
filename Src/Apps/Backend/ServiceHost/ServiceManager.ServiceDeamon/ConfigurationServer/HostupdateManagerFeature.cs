using System;
using System.Reactive.Linq;
using Akka.Actor;
using DynamicData;
using ServiceHost.Client.Shared.ConfigurationServer.Data;
using ServiceHost.Client.Shared.ConfigurationServer.Events;
using ServiceManager.ServiceDeamon.ConfigurationServer.Internal;
using Tauron;
using Tauron.Application.Master.Commands.Administration.Host;
using Tauron.Features;

namespace ServiceManager.ServiceDeamon.ConfigurationServer
{
    public sealed class HostupdateManagerFeature : ActorFeatureBase<HostupdateManagerFeature.State>
    {
        public sealed record State(IObservable<IConfigEvent> EventPublisher, SourceList<string> Hosts, ServerConfigugration ServerConfigugration);

        public static IPreparedFeature New(IObservable<IConfigEvent> publisher, ServerConfigugration serverConfigugration)
            => Feature.Create(() => new HostupdateManagerFeature(), new State(publisher, new SourceList<string>(), serverConfigugration));

        protected override void ConfigImpl()
        {
            CurrentState.Hosts.DisposeWith(this);

            HostApi.CreateOrGet(Context.System)
                   .Event<HostEntryChanged>()
                   .DisposeWith(this);

            (from evt in CurrentState.EventPublisher
             where evt is ServerConfigurationEvent
             from state in UpdateAndSyncActor((ServerConfigurationEvent) evt)
             select state.State with {ServerConfigugration = state.Event.Configugration})
               .AutoSubscribe(UpdateState).DisposeWith(this);

            Receive<HostEntryChanged>(
                obs => from entryChange in obs
                       where !string.IsNullOrWhiteSpace(entryChange.Event.Name)
                       let name = entryChange.Event.Name
                       let remove = entryChange.Event.Removed
                       let state = entryChange.State
                       select state with
                              {
                                  Hosts = state.Hosts.DoAnd(l =>
                                                            {
                                                                if (remove)
                                                                    l.Remove(name);
                                                                else
                                                                    l.Add(name);
                                                            })
                              });

            (from change in CurrentState.Hosts.Connect()
             from item in change.Flatten()
             where item.Reason == ListChangeReason.Add
             select Context.ActorOf(item.Current, HostMonitor.New(item.Current, CurrentState.EventPublisher, CurrentState.ServerConfigugration))
                ).AutoSubscribe(errorHandler: e => Log.Error(e, "Error On Start Host Monitor"))
                 .DisposeWith(this);

            (from change in CurrentState.Hosts.Connect()
             from item in change.Flatten()
             where item.Reason == ListChangeReason.Remove
             let child = Context.Child(item.Current)
             where !child.IsNobody()
             select child
                ).AutoSubscribe(c => Context.Stop(c), errorHandler: e => Log.Error(e, "Error On Stop Host monitor"))
                 .DisposeWith(this);
        }
    }
}