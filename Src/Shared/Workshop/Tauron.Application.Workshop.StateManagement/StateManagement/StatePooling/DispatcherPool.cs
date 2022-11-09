using Tauron.Application.Workshop.Driver;
using Tauron.Application.Workshop.Mutation;

namespace Tauron.Application.Workshop.StateManagement.StatePooling;

public sealed class DispatcherPool
{
    private readonly Dictionary<string, MutatingEngine> _engines = new();

    public MutatingEngine Get(string name, IDriverFactory driverFactory)
    {
        if(_engines.TryGetValue(name, out MutatingEngine? enigine))
            return enigine;

        enigine = MutatingEngine.Create(driverFactory);
        _engines.Add(name, enigine);

        return enigine;
    }
}