using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Linq;
using Akka.Actor;
using Akka.Cluster.Utility;
using Tauron.Features;
using static Tauron.Application.Master.Commands.Administration.Host.InternalHostMessages;
using static Akka.Cluster.Utility.ClusterActorDiscoveryMessage;

namespace Tauron.Application.Master.Commands.Administration.Host;

public sealed class HostApiManagerFeature : ActorFeatureBase<HostApiManagerFeature.ApiState>
{
    public static IPreparedFeature Create()
        => Feature.Create(
            () => new HostApiManagerFeature(),
            _ => new ApiState(ImmutableDictionary<ActorPath, HostEntry>.Empty));
    

    protected override void ConfigImpl()
    {
        Receive<SubscribeFeature.InternalEventSubscription>(ProcessEventSubscription);
        Receive<ActorDown>(OnActorDown);
        Receive<ActorUp>(OnActorUp);
        Receive<GetHostNameResult>(TryGetJostName);
        Receive<IHostApiCommand>(ForwardCommand);

        Start.SubscribeWithStatus(c => ClusterActorDiscovery.Get(c.System).Discovery.Tell(new MonitorActor(HostApi.ApiKey)))
           .DisposeWith(this);
    }

    private IDisposable ForwardCommand(IObservable<StatePair<IHostApiCommand, ApiState>> obs)
    {
        return obs.Select(
                m => new
                     {
                         #pragma warning disable GU0019
                         Command = m.Event, Host = m.State.Entries.FirstOrDefault(e => e.Value.Name == m.Event.Target).Value
                         #pragma warning restore GU0019
                     })
           .SubscribeWithStatus(
                m =>
                {
                    // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                    if(m.Host is null)
                        Context.Sender.Tell(m.Command.CreateDefaultFailed());
                    else
                        m.Host.Actor.Forward(m.Command);
                });
    }

    private IObservable<ApiState> TryGetJostName(IObservable<StatePair<GetHostNameResult, ApiState>> obs)
    {
        return obs.Where(m => m.State.Entries.ContainsKey(Context.Sender.Path))
           .Select(
                m =>
                {
                    (GetHostNameResult message, ApiState state) = m;

                    HostEntry newEntry = state.Entries[Context.Sender.Path] with { Name = message.Name };
                    TellSelf(SendEvent.Create(new HostEntryChanged(newEntry.Name, newEntry.Actor.Path, Removed: false)));

                    return state with { Entries = state.Entries.SetItem(Context.Sender.Path, newEntry) };
                });
    }

    private IObservable<ApiState> OnActorUp(IObservable<StatePair<ActorUp, ApiState>> obs)
    {
        return obs.Select(
            m =>
            {
                (ActorUp message, ApiState state) = m;
                message.Actor.Tell(new GetHostName());
                TellSelf(SendEvent.Create(new HostEntryChanged(HostName.Empty, message.Actor.Path, Removed: false)));

                return state with { Entries = state.Entries.SetItem(message.Actor.Path, new HostEntry(HostName.Empty, message.Actor)) };
            });
    }

    private IObservable<ApiState> OnActorDown(IObservable<StatePair<ActorDown, ApiState>> obs)
    {
        return obs.Where(p => p.State.Entries.ContainsKey(p.Event.Actor.Path))
           .Select(
                m =>
                {
                    (ActorDown actorDown, ApiState state) = m;
                    (HostName name, IActorRef actor) = state.Entries[actorDown.Actor.Path];
                    TellSelf(SendEvent.Create(new HostEntryChanged(name, actor.Path, Removed: true)));

                    return state with { Entries = state.Entries.Remove(actorDown.Actor.Path) };
                });
    }

    private IDisposable ProcessEventSubscription(IObservable<StatePair<SubscribeFeature.InternalEventSubscription, ApiState>> obs)
    {
        return obs.SelectMany(m => m.State.Entries.Select(entry => (entry, m.Event.Intrest)))
           .SubscribeWithStatus(p => p.Intrest.Tell(new HostEntryChanged(p.entry.Value.Name, p.entry.Key, Removed: false)));
    }

    public sealed record ApiState(ImmutableDictionary<ActorPath, HostEntry> Entries);

    public sealed record HostEntry(HostName Name, IActorRef Actor);
}