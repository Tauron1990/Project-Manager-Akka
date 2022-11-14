using Akka.Actor;
using Tauron.Features;
using Tauron.Operations;

namespace Tauron.Application.AkkaNode.Services.Reporting;

public sealed record ReporterEvent<TMessage, TState>(
    Reporter Reporter, TMessage Event, TState State, ITimerScheduler Timer,
    IActorContext Context, IActorRef Sender, IActorRef Parent, IActorRef Self)
{
    public ReporterEvent(Reporter reporter, StatePair<TMessage, TState> @event)
        : this(reporter, @event.Event, @event.State, @event.Timers, @event.Context, @event.Sender, @event.Parent, @event.Self) { }

    public ReporterEvent<TMessage, TState> CompledReporter(IOperationResult result)
    {
        Reporter.Compled(result);

        return this;
    }

    public ReporterEvent<TNewMessage, TState> New<TNewMessage>(TNewMessage newMessage)
        => new(Reporter, newMessage, State, Timer, Context, Sender, Parent, Self);

    public ReporterEvent<TNewMessage, TState> New<TNewMessage>(TNewMessage newMessage, TState state)
        => new(Reporter, newMessage, state, Timer, Context, Sender, Parent, Self);
}