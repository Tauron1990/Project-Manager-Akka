using Akka.Actor;
using JetBrains.Annotations;
using Tauron.Application.Workshop.StateManagement.Akka.Internal;

namespace Tauron.Application.Workshop.StateManagement.Akka;

[PublicAPI]
public abstract class ActorStateBase : ReceiveActor
{
    protected override bool AroundReceive(Receive receive, object message)
    {
        if(message is not StateActorMessage msg)
            return base.AroundReceive(receive, message);

        msg.Apply(this);

        return true;

    }
}