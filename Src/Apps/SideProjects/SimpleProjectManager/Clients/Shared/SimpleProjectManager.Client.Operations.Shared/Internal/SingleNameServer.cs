using Akka.Actor;
using Tauron;

namespace SimpleProjectManager.Client.Operations.Shared.Internal;

public sealed class SingleNameServer : ReceiveActor
{
    private readonly IActorRef _registry;
    private readonly ObjectName _name;
    
    public SingleNameServer(IActorRef registry, in ObjectName name)
    {
        _registry = registry;
        _name = name;
        
        Receive<Terminated>(_ => Context.Stop(Self));
        
        Context.Watch(registry);
    }

    protected override void PreStart()
    {
        _registry.Ask<NameRegistryFeature.RegisterNameResponse>(
                new NameRegistryFeature.RegisterName(_name, Context.Parent),
                TimeSpan.FromSeconds(10))
           .PipeTo
            (
                Context.Parent,
                success: r => new RegistrationResult(r.Duplicate ? default(Duplicate) : default(Success)) { From = _registry },
                failure: ex => new RegistrationResult(ex is AskTimeoutException ? default(Timeout) : ex) { From = _registry }
            )
           .Ignore();
    }
}