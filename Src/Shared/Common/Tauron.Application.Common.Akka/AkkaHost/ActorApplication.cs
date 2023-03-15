using Akka.Actor;
using Stl;

namespace Tauron.AkkaHost;

public static class ActorApplication
{
    private static Option<ActorSystem> _actorSystem;

    public static Option<ActorSystem> ActorSystem
    {
        get => _actorSystem;
        internal set => _actorSystem = value;
    }

    public static IActorRef Deadletter { get; } = new DeadletterSimulator();
    
    public static bool IsStarted => _actorSystem.HasValue;
    
#pragma warning disable MA0097
    private sealed class DeadletterSimulator : MinimalActorRef
#pragma warning restore MA0097
    {
        private static IActorRef GetActor() => ActorSystem.Select(s => s.DeadLetters).GetOrElse(Nobody.Instance);

        protected override void TellInternal(object message, IActorRef sender) => GetActor().Tell(message, sender);

        public override ActorPath Path => GetActor().Path;

        public override IActorRefProvider Provider => ActorSystem
            .Cast<ExtendedActorSystem>()
            .Select(actor => actor.Provider)
            .GetOrElse(() => Nobody.Instance.Provider);
    }
}