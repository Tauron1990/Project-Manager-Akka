using Microsoft.Extensions.DependencyInjection;
using Tauron.Application.Workshop.StateManagement.Builder;
using Tauron.Application.Workshop.StateManagement.Internal;

namespace Tauron.Application.Workshop.StateManagement;

public sealed class StateManagmentModule : IModule
{
    public void Load(IServiceCollection collection)
    {
        collection.AddTransient<IStateInstanceFactory, ActivatorUtilitiesStateFactory>();
        collection.AddTransient<IStateInstanceFactory, SimpleConstructorStateFactory>();
    }
}