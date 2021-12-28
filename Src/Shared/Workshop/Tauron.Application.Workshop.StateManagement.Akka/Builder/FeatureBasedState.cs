using System.Reflection;
using Akka.Util;
using Tauron.Application.Workshop.Mutating;
using Tauron.Application.Workshop.Mutation;
using Tauron.Application.Workshop.StateManagement.Builder;
using Tauron.Application.Workshop.StateManagement.Internal;
using Tauron.Features;

namespace Tauron.Application.Workshop.StateManagement.Akka.Builder;

public class FeatureBasedState : IStateInstanceFactory
{
    public int Order => 1;
    
    public bool CanCreate(Type state)
        => state.Implements<IFeature>();

    public IStateInstance? Create<TData>(Type state, IServiceProvider? serviceProvider, ExtendedMutatingEngine<MutatingContext<TData>> dataEngine, IActionInvoker invoker)
    {
        if (State is null) return null;
        
        var factory = State.GetMethod(
            "Create",
            BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy,
            null,
            CallingConventions.Standard,
            Type.EmptyTypes,
            null);

        if (factory == null)
            return null;

        return FastReflection.Shared.GetMethodInvoker(factory, () => Type.EmptyTypes)(null, null) is not IPreparedFeature feature
            ? null
            : new ActorRefInstance(superviser.CreateCustom(MakeName(), _ => Feature.Props(feature)), State);
    }
}