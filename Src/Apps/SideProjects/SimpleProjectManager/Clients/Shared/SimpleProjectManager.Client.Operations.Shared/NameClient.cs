using Akka.Actor;
using Akka.Cluster.Utility;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using SimpleProjectManager.Client.Operations.Shared.Internal;
using Tauron.TAkka;

namespace SimpleProjectManager.Client.Operations.Shared;

[PublicAPI]
public sealed partial class NameClient : ReceiveActor
{
    private readonly ILogger<NameClient> _logger;
    private readonly ObjectName _name;
    private readonly IObserver<NameState> _nameService;
    private readonly Dictionary<IActorRef, RegistrationResult> _registrys = new();
    private readonly List<NameClientState> _nameClientStates = new();

    public NameClient(ObjectName name, NameService nameService)
    {
        _name = name;
        _nameService = nameService;
        _logger = LoggingProvider.LoggerFactory.CreateLogger<NameClient>();

        UpdateState();
        
        Receive<ClusterActorDiscoveryMessage.ActorUp>(ActorUp);
        Receive<ClusterActorDiscoveryMessage.ActorDown>(ActorDown);

        Receive<RegistrationResult>(HandleResult);
    }

    private void HandleResult(RegistrationResult result)
    {
        _registrys[result.From] = result;
        UpdateState();
    }

    private void ActorDown(ClusterActorDiscoveryMessage.ActorDown down)
    {
        RegistryRemoved(down.Actor.Path);

        _registrys.Remove(down.Actor);
        
        UpdateState();
    }

    private void ActorUp(ClusterActorDiscoveryMessage.ActorUp up)
    {
        NewRegistry(up.Actor.Path);
        if(_registrys.ContainsKey(up.Actor)) return;
        
        _registrys.Add(up.Actor, default(UnInitialized));
        Context.ActorOf(() => new SingleNameServer(up.Actor, _name));
        
        UpdateState();
    }
    
    [LoggerMessage(EventId = 50, Level = LogLevel.Warning, Message = "No Registry Found for Name Request")]
    private partial void NoRegistryFound();

    private void UpdateState()
    {
        _nameClientStates.Clear();

        if(_registrys.Count == 0)
        {
            NoRegistryFound();
            _nameService.OnNext(new NameState(_name, NameClientState.Offline));
            return;
        }
        
        _nameClientStates.AddRange(_registrys.Select(GetState));

        _nameService.OnNext(
            _nameClientStates.Distinct().Take(2).Count() == 1 
                ? new NameState(_name, _nameClientStates[0]) 
                : new NameState(_name, NameClientState.InConsistetent));
    }

    private NameClientState GetState(KeyValuePair<IActorRef, RegistrationResult> result)
        => result.Value.Match(
            _ => NameClientState.Online, 
            _ =>
            {
                NameRegistrationFailed("Request Has Duplicate Entry", result.Key.Path);
                return NameClientState.Duplicate;
            },
            _ =>
            {
                NameRegistrationFailed("Request Has Timed out", result.Key.Path);
                return NameClientState.TimeOut;
            }, 
            _ => NameClientState.UnInitialized,
            ex =>
            {
                ErrorOnRegisterName(ex, result.Key.Path);
                return NameClientState.Failed;
            });

    [LoggerMessage(EventId = 49, Level = LogLevel.Error, Message = "Error on Register Name in Registry. {registry}")]
    private partial void ErrorOnRegisterName(Exception ex, ActorPath registry);
    
    [LoggerMessage(EventId = 44, Level = LogLevel.Information, Message = "New Name Registry {path}")]
    private partial void NewRegistry(ActorPath path);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Name Registry Removed {path}")]
    private partial void RegistryRemoved(ActorPath path);
    
    [LoggerMessage(EventId = 51, Level = LogLevel.Error, Message = "Name Registration Failed:  {message}--{registry}")]
    private partial void NameRegistrationFailed(string message, ActorPath registry);
    
    protected override void PostStop()
    {
        ClusterActorDiscovery.Get(Context.System)
           .UnMonitorActor(new ClusterActorDiscoveryMessage.UnmonitorActor(nameof(NameRegistryFeature)));
        base.PostStop();
    }

    protected override void PreStart()
    {
        ClusterActorDiscovery.Get(Context.System)
           .MonitorActor(new ClusterActorDiscoveryMessage.MonitorActor(nameof(NameRegistryFeature)));
    }
}