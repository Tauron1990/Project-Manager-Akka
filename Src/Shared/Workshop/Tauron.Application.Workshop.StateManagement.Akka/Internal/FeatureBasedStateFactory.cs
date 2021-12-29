using System.Reflection;
using Akka.Util;
using Tauron.AkkaHost;
using Tauron.Application.Workshop.Mutating;
using Tauron.Application.Workshop.Mutation;
using Tauron.Application.Workshop.StateManagement.Builder;
using Tauron.Application.Workshop.StateManagement.Internal;
using Tauron.Features;

namespace Tauron.Application.Workshop.StateManagement.Akka.Internal;

public class FeatureBasedStateFactory : IStateInstanceFactory
{
    public int Order => int.MaxValue - 3;
    
    public bool CanCreate(Type state)
        => state.Implements<IFeature>();

    public IStateInstance? Create<TData>(Type state, IServiceProvider? serviceProvider, ExtendedMutatingEngine<MutatingContext<TData>> dataEngine, IActionInvoker invoker)
    {
        var superviser = WorkspaceSuperviser.Get(ActorApplication.ActorSystem);
        
        var factory = state.GetMethod(
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
            : new ActorRefInstance(superviser.CreateCustom(MakeName<TData>(), _ => Feature.Props(feature)), state);
    }
    
    public static string MakeName<TData>()
        => typeof(TData).Name + "-State";
}