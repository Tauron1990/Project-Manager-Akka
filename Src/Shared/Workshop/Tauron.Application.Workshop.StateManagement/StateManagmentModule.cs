using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Tauron.Application.Workshop.StateManagement.Builder;
using Tauron.Application.Workshop.StateManagement.Internal;

namespace Tauron.Application.Workshop.StateManagement;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
public sealed class StateManagmentModule : IModule
{
    #pragma warning disable GU0011
    public void Load(IServiceCollection collection)
    {
        collection.AddTransient<IStateInstanceFactory, ActivatorUtilitiesStateFactory>();
        collection.AddTransient<IStateInstanceFactory, SimpleConstructorStateFactory>();
    }
}