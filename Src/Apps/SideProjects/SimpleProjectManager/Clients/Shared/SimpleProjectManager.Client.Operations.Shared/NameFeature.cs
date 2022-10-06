using System.Reactive;
using System.Reactive.Linq;
using Akka.Actor;
using Akka.Cluster.Utility;
using Akka.Streams.Implementation.Fusing;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Tauron.Features;
using Tauron.ObservableExt;

namespace SimpleProjectManager.Client.Operations.Shared;

[PublicAPI]
public sealed partial class NameFeature : ActorFeatureBase<NameFeature.State>
{
    private sealed record SignalInit;

    public sealed record State(string Name, bool IsConfirmd, bool IsDown, IActorRef Registry, TaskCompletionSource RegistryWaiter);

    public static IPreparedFeature Create(string name)
        => Feature.Create(() => new NameFeature(), _ => new State(name, false, false, ActorRefs.Nobody, new TaskCompletionSource()));

    protected override void ConfigImpl()
    {
        Receive<ClusterActorDiscoveryMessage.ActorUp>(Handle);
        
        Receive<ClusterActorDiscoveryMessage.ActorDown>(Handle);

        Receive<SignalInit>(obs => obs.Subscribe(m => m.State.RegistryWaiter.TrySetResult()));
        
        Receive<NameRequest>(Handle);
        Receive<NameRegistryFeature.RegisterNameResponse>(obs => obs.Select(ProcessNameResponse));
    }

    [LoggerMessage(EventId = 51, Level = LogLevel.Error, Message = "Name Registration Failed:  {message}")]
    private static partial void NameRegistrationFailed(ILogger logger, string message);
    
    [LoggerMessage(EventId = 50, Level = LogLevel.Warning, Message = "No Registry Found for Name Request")]
    private static partial void NoRegistryFound(ILogger logger);

    [LoggerMessage(EventId = 49, Level = LogLevel.Error, Message = "Error on Register Name in Registry")]
    private static partial void ErrorOnRegisterName(ILogger logger, Exception ex);

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
                            then => UpdateAndSyncActor(then.SelectMany(SendNameRequest)).Select(p => p.State))
            );

    [LoggerMessage(EventId = 55, Level = LogLevel.Warning, Message = "Error on Register Name in Registry with \"{errorMessage}\"")]
    private static partial void NameRegisterError(ILogger log, string errorMessage);

    private async Task<Unit> SendNameRequest(StatePair<NameRequest, State> p)
    {
        try
        {
            var task = p.State.RegistryWaiter.Task;
            if(await Task.WhenAny(task, Task.Delay(TimeSpan.FromSeconds(15))) == task)
            {
                await task;
                p.State.Registry.Tell(new NameRegistryFeature.RegisterName(p.State.Name, p.Sender));

                return Unit.Default;
            }
            
            NoRegistryFound(Logger);
            p.Sender.Tell(new NameResponse(null));
        }
        catch (Exception e)
        {
            ErrorOnRegisterName(Logger, e);

            p.Sender.Tell(new NameResponse(null));
        }
        
        return Unit.Default;
    }

    private State ProcessNameResponse(StatePair<NameRegistryFeature.RegisterNameResponse, State> p)
    {
        var result = p.Event;
        
        if(string.IsNullOrWhiteSpace(result.Error))
        {
            p.Sender.Tell(new NameResponse(result.Name));

            if(string.IsNullOrWhiteSpace(result.Name))
                return p.State with{ IsConfirmd = true, IsDown = false };
        }

        NameRegisterError(Logger, result.Error ?? string.Empty);
        p.Sender.Tell(new NameResponse(null));

        return p.State with { IsConfirmd = false, IsDown = false };
    }

    private IObservable<State> Handle(IObservable<StatePair<ClusterActorDiscoveryMessage.ActorDown, State>> obs)
        => obs.Where(p => p.State.Registry.Equals(p.Event.Actor))
           .Select(p => p.State with { IsDown = true, Registry = ActorRefs.Nobody });

    [LoggerMessage(EventId = 44, Level = LogLevel.Information, Message = "New Name Registry {path}")]
    private static partial void NewRegistry(ILogger logger, ActorPath path);

    [LoggerMessage(EventId = 45, Level = LogLevel.Warning, Message = "New Duplicate Name Registry {path}")]
    private static partial void DuplicateRegistry(ILogger logger, ActorPath path);
    
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