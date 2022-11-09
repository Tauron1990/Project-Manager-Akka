using Akka.Actor;
using Akka.Event;
using JetBrains.Annotations;

namespace Tauron.TAkka;

[PublicAPI]
public static class ObservableActorExtensions
{
    public static FSMBase.State<TS, TD> Replying<TS, TD>(
        this FSMBase.State<TS, TD> state, object msg,
        IActorRef actor, IActorRef sender)
    {
        actor.Tell(msg, sender);

        return state;
    }


    public static FSMBase.State<TS, TD> Replying<TS, TD>(
        this FSMBase.State<TS, TD> state, object msg,
        IActorRef actor)
    {
        actor.Tell(msg);

        return state;
    }

    public static FSMBase.State<TS, TD> ReplyingSelf<TS, TD>(this FSMBase.State<TS, TD> state, object msg)
        => state.Replying(msg, ObservableActor.ExposedContext.Self);

    public static FSMBase.State<TS, TD> ReplyingParent<TS, TD>(this FSMBase.State<TS, TD> state, object msg)
        => state.Replying(msg, ObservableActor.ExposedContext.Parent);

    public static void Receive<TEvent>(this IObservableActor actor, Action<TEvent> handler)
        => actor.Receive<TEvent>(obs => obs.SubscribeWithStatus(handler));

    public static void SubscribeToEvent<TEvent>(
        this IObservableActor actor,
        Func<IObservable<TEvent>, IDisposable> handler)
        => new EventHolder<TEvent>(actor, handler).Register();

    public static void SendEvent<TType>(this IObservableActor actor, TType evt)
        => ObservableActor.ExposedContext.System.EventStream.Publish(evt);

    private readonly struct EventHolder<TEvent>
    {
        private readonly IObservableActor _actor;
        private readonly Func<IObservable<TEvent>, IDisposable> _handler;

        internal EventHolder(IObservableActor actor, Func<IObservable<TEvent>, IDisposable> handler)
        {
            _handler = handler;
            _actor = actor;
        }

        internal void Register()
        {
            _actor.Receive(_handler);


            _actor.Start.Subscribe(context => context.System.EventStream.Subscribe<TEvent>(context.Self))
               .DisposeWith(_actor);
            _actor.Stop.Subscribe(context => context.System.EventStream.Unsubscribe<TEvent>(context.Self))
               .DisposeWith(_actor);
        }
    }
}