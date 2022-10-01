using System.Collections.Immutable;
using System.Reactive.Linq;
using Akka.Actor;
using Microsoft.Extensions.Logging;
using Tauron.Features;

namespace SimpleProjectManager.Client.Operations.Shared;

public sealed class NameRegistry : FeatureActorRefBase<NameRegistry>
{
    public NameRegistry() 
        : base("NameRegistry")
    { }
}

public sealed partial class NameRegistryFeature : ActorFeatureBase<NameRegistryFeature.State>
{
    public sealed record RegisterName;

    public sealed record RegisterNameResponse(string? Error, string? Name);

    public static Func<ILogger<NameRegistryFeature>, IPreparedFeature> Factory()
        => l => Feature.Create(() => new NameRegistryFeature(), new State(l, ImmutableDictionary<string, IActorRef>.Empty));

    public sealed record State(ILogger<NameRegistryFeature> Logger, ImmutableDictionary<string, IActorRef> CurrentClients);

    // ReSharper disable once InconsistentNaming
    private ILogger _logger = null!;

    #region RegisterName

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Actor {path} Terminated. Delete Entry {name}")]
    private partial void ActorTerminated(ActorPath path, string name);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Incomming Register Name Request: {path}")]
    private partial void NewRequest(ActorPath path);

    [LoggerMessage(EventId = 43, Level = LogLevel.Information, Message = "Register Name  Request. No Awnser from Sender: {path}")]
    private partial void NoNameReturned(ActorPath path);

    [LoggerMessage(EventId = 3, Level = LogLevel.Error, Message = "Register Name Request. Error while Ask for Name: {path}")]
    private partial void ErrorAskForName(Exception ex, ActorPath path);

    [LoggerMessage(EventId = 4, Level = LogLevel.Information, Message = "Register Name Request. Name Found: {name} on {path}")]
    private partial void NameFound(string name, ActorPath path);

    [LoggerMessage(EventId = 5, Level = LogLevel.Warning, Message = "Register Name Request. Duplicate Name {name} with {path} found")]
    private partial void DuplicateNameFound(string name, ActorPath path);

    [LoggerMessage(EventId = 6, Level = LogLevel.Information, Message = "{path} Successfully Registrated with {name}")]
    private partial void SuccessfulRegistrated(ActorPath path, string name);

    private IObservable<State> OnTerminated(IObservable<StatePair<Terminated, State>> observable)
        => observable.Select(
            sp =>
            {
                if(!sp.State.CurrentClients.Values.Contains(sp.Event.ActorRef))
                    return sp.State;

                var pair = sp.State.CurrentClients.First(p => sp.Event.ActorRef.Equals(p.Value));
                ActorTerminated(pair.Value.Path, pair.Key);

                return sp.State with { CurrentClients = sp.State.CurrentClients.Remove(pair.Key) };

            });

    private IObservable<State> TryRegisterName(IObservable<StatePair<RegisterName, State>> observable)
        => observable
           .SelectMany(AskForName)
           .Switch()
           .Select(TryRegisterRecivedName);

    private async Task<IObservable<StatePair<(string, IActorRef Sender), State>>> AskForName(StatePair<RegisterName, State> p)
    {
        try
        {
            NewRequest(p.Sender.Path);

            var result = await NameRequest.Ask(p.Sender, p.State.Logger);

            if(string.IsNullOrWhiteSpace(result))
            {
                NoNameReturned(p.Sender.Path);
                p.Sender.Tell(new RegisterNameResponse("Kein Name wurde vom Sender zurülgegeben", null));
            }
            else
            {
                NameFound(result, p.Sender.Path);

                return UpdateAndSyncActor((result, p.Sender));
            }
        }
        catch (Exception e)
        {
            ErrorAskForName(e, p.Sender.Path);
            p.Sender.Tell(new RegisterNameResponse(e.Message, null));
        }

        return UpdateAndSyncActor(((string?)null, p.Sender))!;
    }

    private State TryRegisterRecivedName(StatePair<(string, IActorRef Sender), State> p)
    {
        if(string.IsNullOrWhiteSpace(p.Event.Item1))
            return p.State;

        if(p.State.CurrentClients.ContainsKey(p.Event.Item1))
        {
            DuplicateNameFound(p.Event.Item1, p.Event.Sender.Path);
            p.Sender.Tell(new RegisterNameResponse("Der Name Existiert Schon", null));

            return p.State;
        }

        SuccessfulRegistrated(p.Event.Sender.Path, p.Event.Item1);
        p.Sender.Tell(new RegisterNameResponse(null, p.Event.Item1));
        p.Context.Watch(p.Sender);

        return p.State with { CurrentClients = p.State.CurrentClients.Add(p.Event.Item1, p.Event.Sender) };
    }

    #endregion

    protected override void ConfigImpl()
    {
        _logger = CurrentState.Logger;

        Receive<Terminated>(OnTerminated);
        Receive<RegisterName>(TryRegisterName);
    }
}