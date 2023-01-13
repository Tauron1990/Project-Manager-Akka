using Akka.Actor;
using Akka.Persistence;

namespace SimpleProjectManager.Server.Core.Projections.Core;

public sealed class ProjectionInitializer
{
    private readonly IEnumerable<IInitializeProjection> _projections;
    private readonly ActorSystem _system;

    public ProjectionInitializer(ActorSystem system, IEnumerable<IInitializeProjection> projections)
    {
        _system = system;
        _projections = projections;
    }

    public void Run()
    {
        _system.RegisterExtension(Persistence.Instance);
        _projections.Foreach(p => p.Initialize(_system));
    }
}