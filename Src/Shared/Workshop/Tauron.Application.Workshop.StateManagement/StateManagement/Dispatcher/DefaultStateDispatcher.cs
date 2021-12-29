using JetBrains.Annotations;
using Tauron.Application.Workshop.Driver;

namespace Tauron.Application.Workshop.StateManagement.Dispatcher;

[PublicAPI]
public sealed class DefaultStateDispatcher : IStateDispatcherConfigurator
{
    public IDriverFactory Configurate(IDriverFactory factory)
        => factory;
}