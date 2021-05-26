using System;
using System.Threading.Tasks;
using Akka.Actor;
using JetBrains.Annotations;

namespace Tauron.Akka
{
    [PublicAPI]
    public interface IEventActor
    {
        IActorRef OriginalRef { get; }

        Task<IDisposable> Register(HookEvent hookEvent);

        void Send(IActorRef actor, object send);
    }
}