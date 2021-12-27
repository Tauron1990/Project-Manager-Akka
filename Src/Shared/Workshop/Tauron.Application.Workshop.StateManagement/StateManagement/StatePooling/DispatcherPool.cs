using Akka.Actor;
using Tauron.Application.Workshop.Mutation;

namespace Tauron.Application.Workshop.StateManagement.StatePooling;

public sealed class DispatcherPool
{
    private readonly Dictionary<string, MutatingEngine> _engines = new();

    public MutatingEngine Get(string name, WorkspaceSuperviser superviser, Func<Props, Props>? configure)
    {
        if (_engines.TryGetValue(name, out var enigine))
            return enigine;

        enigine = MutatingEngine.Create(superviser, configure);
        _engines.Add(name, enigine);

        return enigine;
    }
}