using Akka.Actor;
using JetBrains.Annotations;

namespace Tauron.TAkka;

[PublicAPI]
public interface IEventActor
{
    IActorRef OriginalRef { get; }

    Task<IDisposable> Register(HookEvent hookEvent);

    void Send(IActorRef actor, object send);
}