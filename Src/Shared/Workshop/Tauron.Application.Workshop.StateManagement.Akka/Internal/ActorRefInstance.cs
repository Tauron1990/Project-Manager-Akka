using System.Reactive.Threading.Tasks;
using Akka.Actor;
using Akka.Util;
using Tauron.Application.Workshop.Mutating;
using Tauron.Application.Workshop.Mutation;
using Tauron.Application.Workshop.StateManagement.Internal;
using Tauron.Application.Workshop.StateManagement.StatePooling;

namespace Tauron.Application.Workshop.StateManagement.Akka.Internal;

public sealed class ActorRefInstance : IStateInstance
{
    private readonly Type _targetType;
    private bool _initCalled;

    public ActorRefInstance(Task<IActorRef> actorRef, Type targetType)
    {
        ActorRef = actorRef;
        _targetType = targetType;
    }

    private Task<IActorRef> ActorRef { get; }

    public object ActualState => ActorRef.Result;

    public void InitState<TData>(ExtendedMutatingEngine<MutatingContext<TData>> engine)
    {
        if(_targetType.Implements<IInitState<TData>>())
            ActorRef.ToObservable().Subscribe(a => a.Tell(StateActorMessage.Create(engine)));
    }

    public void ApplyQuery<TData>(IExtendedDataSource<MutatingContext<TData>> engine) where TData : class, IStateEntity
    {
        if(_targetType.Implements<IGetSource<TData>>())
            ActorRef.ToObservable().Subscribe(m => m.Tell(StateActorMessage.Create(engine)));
    }

    public void PostInit(IActionInvoker actionInvoker)
    {
        if(_initCalled) return;

        _initCalled = true;

        if(_targetType.Implements<IPostInit>())
            ActorRef.ToObservable().Subscribe(m => m.Tell(StateActorMessage.Create(actionInvoker)));
    }
}