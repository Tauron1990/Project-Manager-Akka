using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using JetBrains.Annotations;

namespace Tauron.Application.Workshop.Mutation
{
    [PublicAPI]
    public sealed class EventSource<TRespond, TData> : EventSourceBase<TRespond>
    {
        public EventSource(WorkspaceSuperviser superviser, Task<IActorRef> mutator, Func<TData, TRespond> transform, Func<TData, bool>? where, IObservable<TData> handler)
            : base(mutator, superviser)
        {
            if (where == null)
                handler.Select(transform).Subscribe(Sender());
            else
                handler.Where(where).Select(transform).Subscribe(Sender());
        }

        public EventSource(WorkspaceSuperviser superviser, Task<IActorRef> mutator, IObservable<TRespond> handler)
            : base(mutator, superviser)
        {
            handler.Subscribe(Sender());
        }
    }
}