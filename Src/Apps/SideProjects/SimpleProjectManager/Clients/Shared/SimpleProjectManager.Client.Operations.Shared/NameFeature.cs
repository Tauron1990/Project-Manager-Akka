using System.Reactive;
using System.Reactive.Linq;
using Akka.Actor;
using Akka.Cluster.Utility;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Tauron;
using Tauron.Application.Workshop.Mutation;
using Tauron.Features;
using Tauron.ObservableExt;

namespace SimpleProjectManager.Client.Operations.Shared;

[PublicAPI]
public sealed partial class NameFeature : ActorFeatureBase<NameFeature.State>
{
    private sealed record SignalInit;

    public sealed record State(string Name, bool IsConfirmd, bool IsDown, IActorRef Registry, TaskCompletionSource<Unit> RegistryWaiter);

    public static IPreparedFeature Create(string name)
        => Feature.Create(() => new NameFeature(), _ => new State(name, false, false, ActorRefs.Nobody, new TaskCompletionSource<Unit>()));

    protected override void ConfigImpl()
    {
        Receive<ClusterActorDiscoveryMessage.ActorUp>(Handle);
        
        Receive<ClusterActorDiscoveryMessage.ActorDown>(Handle);
        
        Receive<NameRequest>(Handle);
    }

    
    
    [LoggerMessage(EventId = 50, Level = LogLevel.Warning, Message = "No Registry Found for Name Request")]
    private partial void NoRegistryFound(ILogger logger);

    [LoggerMessage(EventId = 49, Level = LogLevel.Error, Message = "Error on Register Name in Registry")]
    private partial void ErrorOnRegisterName(ILogger logger, Exception ex);

    private IObservable<State> Handle(IObservable<StatePair<NameRequest, State>> obs)
        => obs.ConditionalSelect()
           .ToResult<State>(
                builder
                    => builder
                       .When(
                            s => s.State.IsConfirmd,
                            then => then.Select(
                                p =>
                                {
                                    p.Sender.Tell(new NameResponse(p.State.Name));

                                    return p.State;
                                }))
                       .When(
                            s => !s.State.IsConfirmd,
                            then => then.SelectMany(
                                async p =>
                                {
                                    try
                                    {
                                        var task = p.State.RegistryWaiter.Task;
                                        if(await Task.WhenAny(task, Task.Delay(TimeSpan.FromSeconds(15))) == task)
                                        {
                                            await task;
                                            var result = await p.State.Registry.Ask<NameRegistryFeature.RegisterNameResponse>(
                                                new NameRegistryFeature.RegisterName(p.State.Name),
                                                TimeSpan.FromSeconds(10));
                                            
                                            
                                        }

                                        Sender.Tell(new NameResponse(null));
                                        NoRegistryFound(Logger);

                                        return p.State;
                                    }
                                    catch (Exception e)
                                    {
                                        ErrorOnRegisterName(Logger, e);

                                        Sender.Tell(new NameResponse(null));

                                        return p.State.Registry;
                                    }
                                }))
            );

    private IObservable<State> Handle(IObservable<StatePair<ClusterActorDiscoveryMessage.ActorDown, State>> obs)
        => obs.Where(p => p.State.Registry.Equals(p.Event.Actor))
           .Select(p => p.State with { IsDown = true, Registry = ActorRefs.Nobody });

    [LoggerMessage(EventId = 44, Level = LogLevel.Information, Message = "New Name Registry {path}")]
    private partial void NewRegistry(ILogger logger, ActorPath path);

    [LoggerMessage(EventId = 45, Level = LogLevel.Warning, Message = "New Duplicate Name Registry {path}")]
    private partial void DuplicateRegistry(ILogger logger, ActorPath path);
    
    private IObservable<State> Handle(IObservable<StatePair<ClusterActorDiscoveryMessage.ActorUp, State>> obs)
        => obs.ConditionalSelect()
           .ToResult<State>(
                builder =>
                    builder.When(
                            p => p.State.Registry.Equals(ActorRefs.Nobody),
                            then => then.Select(
                                p =>
                                {
                                    NewRegistry(Logger, p.Event.Actor.Path);
                                    p.Self.Tell(new SignalInit());

                                    return p.State with { IsDown = false, Registry = p.Event.Actor };
                                }))
                       .When(
                            p => !p.State.Registry.Equals(ActorRefs.Nobody),
                            then => then.Select(
                                p =>
                                {
                                    DuplicateRegistry(Logger, p.Event.Actor.Path);
                                    return p.State;
                                }))
            );

    public override void PostStop()
    {
        ClusterActorDiscovery.Get(Context.System)
           .UnMonitorActor(new ClusterActorDiscoveryMessage.UnmonitorActor(nameof(NameRegistryFeature)));
        base.PostStop();
    }

    public override void PreStart()
    {
        ClusterActorDiscovery.Get(Context.System)
           .MonitorActor(new ClusterActorDiscoveryMessage.MonitorActor(nameof(NameRegistryFeature)));
    }
}