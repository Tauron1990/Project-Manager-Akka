using JetBrains.Annotations;
using Tauron.Application.Workshop.Mutating;
using Tauron.Application.Workshop.Mutating.Changes;

namespace Tauron.Application.Workshop.Mutation;

[PublicAPI]
public static class MutatinEngineExtensions
{
    public static IEventSource<TEvent> EventSource<TData, TEvent>(this IEventSourceable<MutatingContext<TData>> engine)
        where TEvent : MutatingChange
    {
        return engine.EventSource(c => c.GetChange<TEvent>(), c => c.Change?.Cast<TEvent>() != null);
    }
}