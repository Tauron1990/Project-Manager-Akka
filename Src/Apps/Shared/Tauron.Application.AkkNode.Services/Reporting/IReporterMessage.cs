using Akka.Actor;

namespace Tauron.Application.AkkaNode.Services
{
    public interface IReporterMessage
    {
        IActorRef Listner { get; }

        string Info { get; }

        void SetListner(IActorRef listner);
    }
}