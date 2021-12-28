
using Tauron.Application.Workshop.Driver;

namespace Tauron.Application.Workshop.StateManagement.Dispatcher;

public interface IStateDispatcherConfigurator
{
    IDriverFactory Configurate(IDriverFactory factory);
}