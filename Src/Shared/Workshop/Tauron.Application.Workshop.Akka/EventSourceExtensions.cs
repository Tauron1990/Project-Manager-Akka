using Akka.Actor;
using JetBrains.Annotations;
using Tauron.AkkaHost;
using Tauron.Application.Workshop.Mutation;
using Tauron.TAkka;

namespace Tauron.Application.Workshop;

//public sealed class IncommingEvent
//{
//     private IncommingEvent(Action action) => Action = action;
//
//     public Action Action { get; }
//
//     public static IncommingEvent From<TData>(TData data, Action<TData> dataAction)
//     {
//         return new IncommingEvent(() => dataAction(data));
//     }
// }

[PublicAPI]
public static class EventSourceExtensions
{
    public static void RespondOnEventSource<TData>(this IObservableActor actor, IEventSource<TData> eventSource, Action<TData> action)
    {
        eventSource.RespondOn(ObservableActor.ExposedContext.Self);
        actor.Receive<TData>(obs => obs.SubscribeWithStatus(action));
    }

    public static IDisposable RespondOn<TRespond>(this IEventSource<TRespond> eventSource, IActorRef actorRef, WorkspaceSuperviser superviser)
    {
        IDisposable dispo = eventSource.Subscribe(n => actorRef.Tell(n));
        superviser.WatchIntrest(new WatchIntrest(dispo.Dispose, actorRef));

        return dispo;
    }

    public static IDisposable RespondOn<TRespond>(this IEventSource<TRespond> eventSource, IActorRef? source, Action<TRespond> action, WorkspaceSuperviser superviser)
    {
        if(source.IsNobody())
            return eventSource.Subscribe(action);

        IDisposable dispo = eventSource.Subscribe(t => source.Tell(new ObservableActor.TransmitAction(() => action(t))));
        superviser.WatchIntrest(new WatchIntrest(dispo.Dispose, source!));

        return dispo;
    }

    public static IDisposable RespondOn<TRespond>(this IEventSource<TRespond> eventSource, IActorRef? source, Action<TRespond> action)
        => RespondOn(eventSource, source, action, WorkspaceSuperviser.Get(ActorApplication.ActorSystem));

    public static IDisposable RespondOn<TRespond>(this IEventSource<TRespond> eventSource, IActorRef actorRef)
        => RespondOn(eventSource, actorRef, WorkspaceSuperviser.Get(ActorApplication.ActorSystem));
}