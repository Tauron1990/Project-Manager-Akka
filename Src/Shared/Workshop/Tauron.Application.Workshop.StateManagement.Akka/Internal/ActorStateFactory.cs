using Akka.Actor;
using Akka.Util;
using Tauron.AkkaHost;
using Tauron.Application.Workshop.Mutating;
using Tauron.Application.Workshop.Mutation;
using Tauron.Application.Workshop.StateManagement.Builder;
using Tauron.Application.Workshop.StateManagement.Internal;

namespace Tauron.Application.Workshop.StateManagement.Akka.Internal;

public sealed class ActorStateFactory : IStateInstanceFactory
{
    public int Order => int.MaxValue - 2;
    public bool CanCreate(Type state)
        => state.Implements<ActorStateBase>();

    public IStateInstance Create<TData>(Type state, IServiceProvider? serviceProvider, ExtendedMutatingEngine<MutatingContext<TData>> dataEngine, IActionInvoker invoker)
    {
        var superviser = WorkspaceSuperviser.Get(ActorApplication.ActorSystem);
        return new ActorRefInstance(superviser.CreateCustom(FeatureBasedStateFactory.MakeName<TData>(), _ => Props.Create(state)), state);
    }
}