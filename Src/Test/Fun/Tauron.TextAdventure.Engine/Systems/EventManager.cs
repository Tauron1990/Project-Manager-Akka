using System.Reactive.Linq;
using System.Reactive.Subjects;
using JetBrains.Annotations;
using Tauron.TextAdventure.Engine.Data;

namespace Tauron.TextAdventure.Engine.Systems;

[PublicAPI]
public sealed class EventManager : IDisposable
{
    private EventStore? _store;

    private EventStore EventStore
    {
        get
        {
            if(_store is null)
                throw new InvalidOperationException("Game is not Initialized (Event Store)");

            return _store;
        }
    }

    private readonly Subject<object> _dispatcher = new();

    internal void Initialize(EventStore eventStore)
        => _store = eventStore;


    internal void Dipatch(IEnumerable<object> toDispatch)
    {
        foreach(object evt in toDispatch)
            _dispatcher.OnNext(evt);
    }

    public IObservable<TEvent> OnEvent<TEvent>()
        where TEvent : IEvent
        => _dispatcher.OfType<TEvent>();

    public IObservable<TCommand> OnCommand<TCommand>()
        where TCommand : IGameCommand
        => _dispatcher.OfType<TCommand>();

    public void Dispose()
        => _dispatcher.Dispose();
}