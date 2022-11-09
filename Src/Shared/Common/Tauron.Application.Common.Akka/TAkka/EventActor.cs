using System.Reactive;
using System.Reactive.Disposables;
using Akka.Actor;
using Akka.Event;
using JetBrains.Annotations;

namespace Tauron.TAkka;

[PublicAPI]
public sealed class EventActor : UntypedActor
{
    private readonly ILoggingAdapter _log = Context.GetLogger();

    private readonly Dictionary<Type, Delegate?> _registrations = new();
    private bool _killOnFirstRespond;

    public EventActor(bool killOnFirstRespond) => _killOnFirstRespond = killOnFirstRespond;

    public static IEventActor From(IActorRef actorRef) => new HookEventActor(actorRef);

    public static IEventActor Create(IActorRefFactory system, string? name)
        => Create<Unit>(system, name, null, false);

    public static IEventActor CreateSelfKilling(IActorRefFactory system, string? name)
        => CreateSelfKilling<Unit>(system, name, null);

    public static IEventActor Create<TPayload>(IActorRefFactory system, string? name, Action<TPayload>? handler)
        => Create(system, name, handler, false);

    public static IEventActor CreateSelfKilling<TPayload>(IActorRefFactory system, string? name, Action<TPayload>? handler)
        => Create(system, name, handler, true);

    private static IEventActor Create<TPayload>(IActorRefFactory system, string? name, Action<TPayload>? handler, bool killOnFirstResponse)
    {
        var temp = new HookEventActor(system.ActorOf(Props.Create(() => new EventActor(false)), name));
        if(handler is not null)
            temp.Register(HookEvent.Create(handler)).Ignore();

        return temp;
    }

    protected override void OnReceive(object message)
    {
        switch (message)
        {
            case HookEvent hookEvent:
                RegisterEvent(hookEvent);

                break;
            case RemoveDel remove:
                RemoveDelegate(remove);

                break;
            default:
                DefaultHandler(message);

                break;
        }
    }

    private void RemoveDelegate(RemoveDel remove)
    {
        (Delegate @delegate, Type target) = remove;

        if(!_registrations.TryGetValue(target, out Delegate? action)) return;

        if(action == @delegate)
            _registrations.Remove(target);
        else
            _registrations[target] = action.Remove(@delegate);
    }

    private void RegisterEvent(HookEvent hookEvent)
    {
        (Delegate @delegate, Type target) = hookEvent;
        if(_registrations.TryGetValue(target, out Delegate? del))
            del = Delegate.Combine(del, @delegate);
        else
            del = @delegate;

        _registrations[target] = del;

        Sender.Tell(Disposable.Create((Self, Del: del, Target: target), info => info.Self.Tell(new RemoveDel(info.Del, info.Target))));
    }

    private void DefaultHandler(object message)
    {
        Type msgType = message.GetType();
        if(_registrations.TryGetValue(msgType, out Delegate? callDel))
        {
            try
            {
                callDel?.DynamicInvoke(message);
            }
            catch (Exception exception)
            {
                _log.Error(exception, "Error On Event Hook Execution");
            }

            if(_killOnFirstRespond)
                Context.Stop(Context.Self);
        }
        else
        {
            Unhandled(message);
        }
    }

    private sealed record RemoveDel(Delegate Delegate, Type Target);

    private sealed class HookEventActor : IEventActor
    {
        internal HookEventActor(IActorRef actorRef) => OriginalRef = actorRef;

        public IActorRef OriginalRef { get; }

        public Task<IDisposable> Register(HookEvent hookEvent) => OriginalRef.Ask<IDisposable>(hookEvent);

        public void Send(IActorRef actor, object send)
        {
            actor.Tell(send, OriginalRef);
        }
    }
}