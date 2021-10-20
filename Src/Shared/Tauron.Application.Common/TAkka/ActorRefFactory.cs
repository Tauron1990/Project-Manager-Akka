using Akka.Actor;
using Akka.DependencyInjection;
using JetBrains.Annotations;

namespace Tauron.TAkka;

[PublicAPI]
public sealed class ActorRefFactory<TActor> where TActor : ActorBase
{
    private readonly ActorSystem _system;

    public ActorRefFactory(ActorSystem system) => _system = system;

    public IActorRef Create(string? name = null) => _system.ActorOf(CreateProps(), name);

    public IActorRef CreateSync(string? name = null) => _system.ActorOf(CreateSyncProps(), name);

    public Props CreateProps()
        => _system.GetExtension<DependencyResolver>().Props<TActor>();

    public Props CreateSyncProps()
        => CreateProps().WithDispatcher("synchronized-dispatcher");
}