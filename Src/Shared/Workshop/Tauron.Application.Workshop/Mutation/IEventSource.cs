using System;
using Akka.Actor;

namespace Tauron.Application.Workshop.Mutation;

public sealed class IncommingEvent
{
    private IncommingEvent(Action action) => Action = action;

    public Action Action { get; }

    public static IncommingEvent From<TData>(TData data, Action<TData> dataAction)
    {
        return new IncommingEvent(() => dataAction(data));
    }
}

public interface IEventSource<out TRespond> : IObservable<TRespond>
{
    IDisposable RespondOn(IActorRef actorRef);

    IDisposable RespondOn(IActorRef? source, Action<TRespond> action);
}