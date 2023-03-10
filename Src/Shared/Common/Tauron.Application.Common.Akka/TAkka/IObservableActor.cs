using System.Reactive;
using Akka.Actor;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Tauron.TAkka;

[PublicAPI]
public interface IObservableActor : IResourceHolder
{
    IObservable<IActorContext> Start { get; }

    IObservable<IActorContext> Stop { get; }

    ILogger Log { get; }
    IActorRef Self { get; }
    IActorRef Parent { get; }
    IActorRef? Sender { get; }
    
    void Receive<TEvent>(Func<IObservable<TEvent>, IObservable<Unit>> handler);
    
    void Receive<TEvent>(Func<IObservable<TEvent>, IObservable<TEvent>> handler);
    void Receive<TEvent>(Func<IObservable<TEvent>, IObservable<Unit>> handler, Func<Exception, bool> errorHandler);

    void Receive<TEvent>(
        Func<IObservable<TEvent>, IObservable<TEvent>> handler,
        Func<Exception, bool> errorHandler);

    void Receive<TEvent>(Func<IObservable<TEvent>, IDisposable> handler);

    void Become(Action configure);

    void BecomeStacked(Action configure);
    
    void UnbecomeStacked();
}