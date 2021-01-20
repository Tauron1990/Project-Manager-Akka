﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Linq;
using Akka.Actor;
using Akka.Cluster.Utility;
using Tauron.Akka;
using Tauron.Application.AkkNode.Services.Core;
using Tauron.Application.Master.Commands.Administration.Host;
using Tauron.Features;
using static Tauron.Application.Master.Commands.Administration.Host.InternalHostMessages;
using static Akka.Cluster.Utility.ClusterActorDiscoveryMessage;

namespace Tauron.Application.Master.Commands.Administration.Host
{
    public sealed class HostApiManagerFeature : ActorFeatureBase<HostApiManagerFeature.ApiState>
    {
        public sealed record ApiState(ImmutableDictionary<ActorPath, HostEntry> Entries);

        public static IPreparedFeature Create()
            => Feature.Create(() => new HostApiManagerFeature(), () => new ApiState(ImmutableDictionary<ActorPath, HostEntry>.Empty));

        protected override void Config()
        {
            Receive<SubscribeFeature.InternalEventSubscription>(obs => obs.SelectMany(m => m.State.Entries.Select(entry => (entry, m.Event.Intrest)))
                                                                          .SubscribeWithStatus(p => p.Intrest.Tell(new HostEntryChanged(p.entry.Value.Name, p.entry.Key, false))));

            Receive<ActorDown>(obs => obs.Where(p => p.State.Entries.ContainsKey(p.Event.Actor.Path))
                                         .Select(m =>
                                                 {
                                                     var (actorDown, state, _) = m;
                                                     var (name, actor) = state.Entries[actorDown.Actor.Path];
                                                     TellSelf(SendEvent.Create(new HostEntryChanged(name, actor.Path, true)));

                                                     return state with{ Entries = state.Entries.Remove(actorDown.Actor.Path)};
                                                 }));

            Receive<ActorUp>(obs => obs.Select(m =>
                                               {
                                                   var (message, state, _) = m;
                                                   message.Actor.Tell(new GetHostName());
                                                   TellSelf(SendEvent.Create(new HostEntryChanged(string.Empty, message.Actor.Path, false)));

                                                   return state with {Entries = state.Entries.SetItem(message.Actor.Path, new HostEntry(string.Empty, message.Actor))};
                                               }));

            Receive<GetHostNameResult>(obs => obs.Where(m => m.State.Entries.ContainsKey(Context.Sender.Path))
                                                 .Select(m =>
                                                         {
                                                             var (message, state, _) = m;

                                                             var newEntry = state.Entries[Context.Sender.Path] with {Name = message.Name};
                                                             TellSelf(SendEvent.Create(new HostEntryChanged(newEntry.Name, newEntry.Actor.Path, false)));

                                                             return state with {Entries = state.Entries.SetItem(Context.Sender.Path, newEntry)};
                                                         }));

            Receive<CommandBase>(obs => obs.Select(m => new {Command = m.Event, Host = m.State.Entries.FirstOrDefault(e => e.Value.Name == m.Event.Target).Value})
                                           .SubscribeWithStatus(m =>
                                                                {
                                                                    if(m.Host == null)
                                                                        Context.Sender.Tell(new OperationResponse(false));
                                                                    else
                                                                        m.Host.Actor.Forward(m.Command);
                                                                }));

            Start.SubscribeWithStatus(c => ClusterActorDiscovery.Get(c.System).Discovery.Tell(new MonitorActor(HostApi.ApiKey)));
        }

        public sealed record HostEntry(string Name, IActorRef Actor);
    }
}