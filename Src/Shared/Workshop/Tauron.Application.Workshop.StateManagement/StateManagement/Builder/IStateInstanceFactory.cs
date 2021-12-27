using Tauron.Application.Workshop.StateManagement.Internal;

namespace Tauron.Application.Workshop.StateManagement.Builder;

public interface IStateInstanceFactory
{
    bool CanCreate(Type state);

    IStateInstance Create(IStateInstanceFactory[] instanceFactories, IServiceProvider? serviceProvider);
}