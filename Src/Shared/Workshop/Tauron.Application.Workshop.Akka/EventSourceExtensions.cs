using Akka.Actor;
using JetBrains.Annotations;
using Stl;
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
        #pragma warning disable GU0011
        eventSource.RespondOn(ObservableActor.ExposedContext.Self);
        actor.Receive<TData>(obs => obs.SubscribeWithStatus(action));
    }

    public static Option<IDisposable> RespondOn<TRespond>(this IEventSource<TRespond> eventSource, IActorRef actorRef, Option<WorkspaceSuperviser> superviser)
    {
        return superviser.Select(RunSubscribe);
        
        IDisposable RunSubscribe(WorkspaceSuperviser workspaceSuperviser)
    {
        IDisposable dispo = eventSource.Subscribe(n => actorRef.Tell(n));
        workspaceSuperviser.WatchIntrest(new WatchIntrest(dispo.Dispose, actorRef));

        return dispo;
    }
    
    }

    public static Option<IDisposable> RespondOn<TRespond>(this IEventSource<TRespond> eventSource, IActorRef? source, Action<TRespond> action, Option<WorkspaceSuperviser> superviser)
    {
        return source.IsNobody() 
            ? Option.Some(eventSource.Subscribe(action)) 
            : superviser.Select(RunSubscribe);

        IDisposable RunSubscribe(WorkspaceSuperviser workspaceSuperviser)
        {
            IDisposable dispo = eventSource.Subscribe(t => source.Tell(new ObservableActor.TransmitAction(() => action(t))));
            workspaceSuperviser.WatchIntrest(new WatchIntrest(dispo.Dispose, source!));

            return dispo;
        }
    }

    public static Option<IDisposable> RespondOn<TRespond>(this IEventSource<TRespond> eventSource, IActorRef? source, Action<TRespond> action)
        => RespondOn(eventSource, source, action, WorkspaceSuperviser.Get(ActorApplication.ActorSystem));

    public static Option<IDisposable> RespondOn<TRespond>(this IEventSource<TRespond> eventSource, IActorRef actorRef)
        => RespondOn(eventSource, actorRef, WorkspaceSuperviser.Get(ActorApplication.ActorSystem));
}