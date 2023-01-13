using Akka.Actor;

namespace Tauron;

public class GenericSupervisorActor : ReceiveActor
{
    private readonly SupervisorStrategy? _supervisorStrategy;

    public GenericSupervisorActor(SupervisorStrategy? supervisorStrategy)
    {
        _supervisorStrategy = supervisorStrategy;

        Receive<CreateActor>(c => Sender.Tell(new CreateActorResult(Context.ActorOf(c.Props, c.Name))));
    }

    protected override SupervisorStrategy SupervisorStrategy()
        => _supervisorStrategy ?? Akka.Actor.SupervisorStrategy.DefaultStrategy;

    public sealed record CreateActor(string? Name, Props Props);

    public sealed record CreateActorResult(IActorRef Actor);
}