using Akka.Actor;

namespace Tauron.Application.Workshop;

internal class SuprvisorExt : IExtension
{
    private readonly ActorSystem _system;
    private WorkspaceSuperviser? _superviser;

    internal SuprvisorExt(ActorSystem system)
        => _system = system;

    internal WorkspaceSuperviser GetOrInit(string name)
    {
        if(_superviser is not null) return _superviser;

        _superviser = new WorkspaceSuperviser(_system, name);

        return _superviser;
    }
}