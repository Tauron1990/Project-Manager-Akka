using System.Collections.Immutable;
using System.Reactive.Linq;
using Akka.Actor;
using Microsoft.Extensions.Logging;
using Tauron.Features;

namespace SimpleProjectManager.Client.Operations.Shared;

public sealed partial class NameRegistryFeature : ActorFeatureBase<NameRegistryFeature.State>
{
    // ReSharper disable once InconsistentNaming
    private ILogger _logger = null!;

    public static Func<ILogger<NameRegistryFeature>, IPreparedFeature> Factory()
        => l => Feature.Create(() => new NameRegistryFeature(), new State(l, ImmutableDictionary<ObjectName, IActorRef>.Empty));

    protected override void ConfigImpl()
    {
        _logger = CurrentState.Logger;

        Receive<Terminated>(OnTerminated);
        Receive<RegisterName>(TryRegisterName);
    }

    public sealed record RegisterName(ObjectName Name, IActorRef From);

    public sealed record RegisterNameResponse(bool Duplicate, ObjectName? Name, IActorRef From);

    public sealed record State(ILogger<NameRegistryFeature> Logger, ImmutableDictionary<ObjectName, IActorRef> CurrentClients);

    #region RegisterName

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Actor {path} Terminated. Delete Entry {name}")]
    private partial void ActorTerminated(ActorPath path, in ObjectName name);

    // [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Incomming Register Name Request: {path}")]
    // private partial void NewRequest(ActorPath path);
    //
    // [LoggerMessage(EventId = 43, Level = LogLevel.Information, Message = "Register Name  Request. No Awnser from Sender: {path}")]
    // private partial void NoNameReturned(ActorPath path);
    //
    // [LoggerMessage(EventId = 3, Level = LogLevel.Error, Message = "Register Name Request. Error while Ask for Name: {path}")]
    // private partial void ErrorAskForName(Exception ex, ActorPath path);
    //
    // [LoggerMessage(EventId = 4, Level = LogLevel.Information, Message = "Register Name Request. Name Found: {name} on {path}")]
    // private partial void NameFound(string name, ActorPath path);

    [LoggerMessage(EventId = 5, Level = LogLevel.Warning, Message = "Register Name Request. Duplicate Name {name} with {path} found")]
    private partial void DuplicateNameFound(in ObjectName name, ActorPath path);

    [LoggerMessage(EventId = 6, Level = LogLevel.Information, Message = "{path} Successfully Registrated with {name}")]
    private partial void SuccessfulRegistrated(ActorPath path, in ObjectName name);

    private IObservable<State> OnTerminated(IObservable<StatePair<Terminated, State>> observable)
        => observable.Select(
            sp =>
            {
                if(!sp.State.CurrentClients.ContainsValue(sp.Event.ActorRef))
                    return sp.State;

                var pair = sp.State.CurrentClients.First(p => sp.Event.ActorRef.Equals(p.Value));
                ActorTerminated(pair.Value.Path, pair.Key);

                return sp.State with { CurrentClients = sp.State.CurrentClients.Remove(pair.Key) };

            });

    private IObservable<State> TryRegisterName(IObservable<StatePair<RegisterName, State>> observable)
        => observable.Select(TryRegisterRecivedName);

    private State TryRegisterRecivedName(StatePair<RegisterName, State> p)
    {
        if(p.State.CurrentClients.ContainsKey(p.Event.Name))
        {
            DuplicateNameFound(p.Event.Name, p.Sender.Path);
            p.Sender.Tell(new RegisterNameResponse(Duplicate: true, Name: null, p.Event.From));

            return p.State;
        }

        SuccessfulRegistrated(p.Sender.Path, p.Event.Name);
        p.Sender.Tell(new RegisterNameResponse(Duplicate: false, p.Event.Name, p.Event.From));
        p.Context.Watch(p.Sender);

        return p.State with { CurrentClients = p.State.CurrentClients.Add(p.Event.Name, p.Sender) };
    }

    #endregion
}