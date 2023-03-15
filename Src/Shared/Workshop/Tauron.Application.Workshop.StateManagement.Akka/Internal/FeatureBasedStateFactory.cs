using System.Reflection;
using Stl;
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
        => state.IsAssignableTo(typeof(IFeature));

    public Option<IStateInstance> Create<TData>(
        Type state, IServiceProvider? serviceProvider, ExtendedMutatingEngine<MutatingContext<TData>> dataEngine, IActionInvoker invoker)
    {
        var superviser = WorkspaceSuperviser.Get(ActorApplication.ActorSystem);

        MethodInfo? factory = state.GetMethod(
            "Create",
            BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy,
            binder: null,
            CallingConventions.Standard,
            Type.EmptyTypes,
            modifiers: null);

        if(factory is null)
            return Option<IStateInstance>.None;

        return FastReflection.Shared
            .GetMethodInvoker(factory, () => Type.EmptyTypes)(arg1: null, arg2: null) is not IPreparedFeature feature
            ? Option<IStateInstance>.None
            : new ActorRefInstance(superviser.Select(s => s.CreateCustom(MakeName<TData>(), _ => Feature.Props(feature))), state);
    }

    public static string MakeName<TData>()
        => typeof(TData).Name + "-State";
}