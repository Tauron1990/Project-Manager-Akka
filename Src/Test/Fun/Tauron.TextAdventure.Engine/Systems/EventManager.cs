using System.Collections.Immutable;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using JetBrains.Annotations;
using Tauron.TextAdventure.Engine.Data;

namespace Tauron.TextAdventure.Engine.Systems;

[PublicAPI]
public sealed class EventManager : IDisposable
{
    private EventStore? _store;
    private ImmutableDictionary<Type, IEventProcessor> _eventProcessors = ImmutableDictionary<Type, IEventProcessor>.Empty;
    private readonly Subject<object> _dispatcher = new();

    internal ImmutableList<Action<GameState>> Init { get; set; }
    
    internal EventStore EventStore
    {
        get
        {
            if(_store is null)
                throw new InvalidOperationException("Game is not Initialized (Event Store)");

            return _store;
        }
    }

    public GameState GameState => EventStore.GameState;
    
    internal void RegisterEvent<TEvent>(Action<GameState, TEvent> processor) 
        where TEvent : IEvent
        => _eventProcessors = _eventProcessors.Add(typeof(TEvent), new EventProcessor<TEvent>(_dispatcher, processor));

    internal void Save()
        => _store.SaveGame();

    internal void Initialize(EventStore eventStore)
    {
        _store = eventStore;

        foreach (var action in Init)
            action(GameState);
    }

    internal void Free()
        => _store = null;
    
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

    public void SendCommand(IGameCommand command)
        => _dispatcher.OnNext(command);

    public IObserver<TType> SendCommand<TType>()
        where TType : IGameCommand
        => Observer.Create<TType>(c => SendCommand(c), _dispatcher.OnError, _dispatcher.OnCompleted);

    public void StoreEvent<TEvent>(TEvent evt)
        where TEvent : IEvent
        => _eventProcessors[typeof(TEvent)].Process(EventStore, evt);

    public IObserver<TEvent> StoreEvent<TEvent>()
        where TEvent : IEvent
        => Observer.Create<TEvent>(StoreEvent, _dispatcher.OnError, _dispatcher.OnCompleted);

    public void Dispose()
        => _dispatcher.Dispose();
    
    private interface IEventProcessor
    {
        void Process(EventStore store, IEvent evt);
    }
    
    private class EventProcessor<TEvent> : IEventProcessor
        where TEvent : IEvent
    {
        private readonly Subject<object> _dispatcher;
        private readonly Action<GameState, TEvent> _processor;

        public EventProcessor(Subject<object> dispatcher, Action<GameState, TEvent> processor)
        {
            _dispatcher = dispatcher;
            _processor = processor;
        }

        public void Process(EventStore store, IEvent evt)
        {
            store.ApplyEvent((TEvent)evt, _processor);
            _dispatcher.OnNext(evt);
        }
    }
}