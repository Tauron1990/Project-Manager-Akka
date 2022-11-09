using Akka.Actor;

namespace Tauron.AkkaHost;

public static class ActorApplication
{
    private static ActorSystem? _actorSystem;

    public static ActorSystem ActorSystem
    {
        get
        {
            if(_actorSystem is null)
                throw new InvalidOperationException("An Actorsystem was not set");

            return _actorSystem;
        }
        internal set => _actorSystem = value;
    }

    public static bool IsStarted => _actorSystem != null;
}