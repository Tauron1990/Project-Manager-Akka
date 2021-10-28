using Akka.Actor;

namespace SimpleProjectManager.Server.Core.Projections.Core;

public interface IInitializeProjection
{
    void Initialize(ActorSystem system);
}