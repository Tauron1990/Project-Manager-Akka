using System;
using JetBrains.Annotations;
using Tauron.Akka;
using Tauron.Application.Workshop.Mutation;

namespace Tauron.Application.Workshop
{
    [PublicAPI]
    public static class EventSourceExtensions
    {
        public static void RespondOnEventSource<TData>(this IExpandedReceiveActor actor, IEventSource<TData> eventSource, Action<TData> action)
        {
            eventSource.RespondOn(ObservableActor.ExposedContext.Self);
            actor.Exposed.Receive<TData>((data, _) => action(data));
        }

        public static void RespondOn<TData>(this IEventSource<TData> source, Action<TData> action)
        {
            source.RespondOn(null, action);
        }

        public static MutateClass<TRecieve, TMutator> Mutate<TRecieve, TMutator>(
            this IObservable<TRecieve> selector, TMutator mutator)
            => new(mutator, selector);


        #region Mutate

        public sealed class MutateClass<TRecieve, TMutator>
        {
            private readonly TMutator _mutator;
            private readonly IObservable<TRecieve> _selector;

            public MutateClass(TMutator mutator, IObservable<TRecieve> selector)
            {
                _mutator = mutator;
                _selector = selector;
            }

            public IObservable<TNext> With<TNext>(Func<TMutator, IEventSource<TNext>> eventSource, Func<TMutator, Action<TRecieve>> run)
            {
                With(run);
                return eventSource(_mutator);
            }

            public IDisposable With(Func<TMutator, Action<TRecieve>> run) => _selector.Subscribe(run(_mutator));
        }

        #endregion
    }
}