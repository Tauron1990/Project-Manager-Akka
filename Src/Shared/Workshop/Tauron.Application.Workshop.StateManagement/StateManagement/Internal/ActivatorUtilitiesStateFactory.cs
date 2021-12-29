using Microsoft.Extensions.DependencyInjection;
using Tauron.Application.Workshop.Mutating;
using Tauron.Application.Workshop.Mutation;
using Tauron.Application.Workshop.StateManagement.Builder;

namespace Tauron.Application.Workshop.StateManagement.Internal;

public sealed class ActivatorUtilitiesStateFactory : IStateInstanceFactory
{
    public int Order => int.MaxValue - 1;
    public bool CanCreate(Type state)
        => true;

    public IStateInstance? Create<TData>(Type state, IServiceProvider? serviceProvider, ExtendedMutatingEngine<MutatingContext<TData>> dataEngine, IActionInvoker invoker)
        => serviceProvider is null ? null : new PhysicalInstance(ActivatorUtilities.CreateInstance(serviceProvider, state, dataEngine, invoker));
}