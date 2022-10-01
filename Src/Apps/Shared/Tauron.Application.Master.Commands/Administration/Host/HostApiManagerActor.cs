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
        Receive<SubscribeFeature.InternalEventSubscription>(
            obs => obs
               .SelectMany(m => m.State.Entries.Select(entry => (entry, m.Event.Intrest)))
               .SubscribeWithStatus(
                    p
                        => p.Intrest.Tell(new HostEntryChanged(p.entry.Value.Name, p.entry.Key, Removed: false))));

        Receive<ActorDown>(
            obs => obs.Where(p => p.State.Entries.ContainsKey(p.Event.Actor.Path))
               .Select(
                    m =>
                    {
                        var (actorDown, state, _) = m;
                        var (name, actor) = state.Entries[actorDown.Actor.Path];
                        TellSelf(SendEvent.Create(new HostEntryChanged(name, actor.Path, Removed: true)));

                        return state with { Entries = state.Entries.Remove(actorDown.Actor.Path) };
                    }));

        Receive<ActorUp>(
            obs => obs.Select(
                m =>
                {
                    var (message, state, _) = m;
                    message.Actor.Tell(new GetHostName());
                    TellSelf(SendEvent.Create(new HostEntryChanged(string.Empty, message.Actor.Path, Removed: false)));

                    return state with
                           {
                               Entries = state.Entries.SetItem(message.Actor.Path, new HostEntry(string.Empty, message.Actor))
                           };
                }));

        Receive<GetHostNameResult>(
            obs => obs.Where(m => m.State.Entries.ContainsKey(Context.Sender.Path))
               .Select(
                    m =>
                    {
                        var (message, state, _) = m;

                        var newEntry = state.Entries[Context.Sender.Path] with { Name = message.Name };
                        TellSelf(SendEvent.Create(new HostEntryChanged(newEntry.Name, newEntry.Actor.Path, Removed: false)));

                        return state with { Entries = state.Entries.SetItem(Context.Sender.Path, newEntry) };
                    }));

        Receive<IHostApiCommand>(
            obs => obs
               .Select(
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
                        if (m.Host is null)
                            Context.Sender.Tell(m.Command.CreateDefaultFailed());
                        else
                            m.Host.Actor.Forward(m.Command);
                    }));

        Start.SubscribeWithStatus(c => ClusterActorDiscovery.Get(c.System).Discovery.Tell(new MonitorActor(HostApi.ApiKey)))
           .DisposeWith(this);
    }

    public sealed record ApiState(ImmutableDictionary<ActorPath, HostEntry> Entries);

    public sealed record HostEntry(string Name, IActorRef Actor);
}