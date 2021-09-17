using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Akka;
using Akka.Streams;
using Akka.Actor;
using Akka.Streams.Dsl;
using ServiceHost.Client.Shared.ConfigurationServer;
using ServiceHost.Client.Shared.ConfigurationServer.Events;
using Tauron;
using Tauron.Application;
using Tauron.Application.AkkaNode.Services.Core;
using Tauron.Features;

namespace ServiceManager.Server.AppCore.Helper
{
    public interface IEventDispatcher : IFeatureActorRef<IEventDispatcher>
    {
        void Init();
    }
    
    public abstract class EventDispatcherRef : FeatureActorRefBase<IEventDispatcher>, IEventDispatcher
    {
        protected EventDispatcherRef(string? name) : base(name) { }
        
        public void Init() => Tell(new InitActor());
    }
    
    public abstract class RestartingEventDispatcherActorBase<TEventType, TEventSource> :ActorFeatureBase<RestartingEventDispatcherActorBase<TEventType, TEventSource>.State>
        where TEventSource : IQueryIsAliveSupport
    {
        public sealed record State(TEventSource EventSource, Func<TEventSource, Task<Source<TEventType, NotUsed>>> GetEventSource, AggregateEvent<TEventType> Dispatcher);
        
        protected override void ConfigImpl()
        {
            var materializer = Context.Materializer();

            Receive<InitActor>(obs => obs.ToUnit(p => p.Self.Tell(new TryQueryEventSource())));

            Receive<TryQueryEventSource>(
                obs => (from r in obs
                        let system = r.Context.System

                        let source = r.State.EventSource
                        let getSource = r.State.GetEventSource

                        from cr in source.QueryIsAlive(system, TimeSpan.FromSeconds(10))
                        from cs in cr.IsAlive
                            ? getSource(source)
                            : Task.FromException<Source<TEventType, NotUsed>>(new InvalidOperationException("Service not Alive"))

                        select cs)
                   .AutoSubscribe(
                        s => s.RunForeach(evt => CurrentState.Dispatcher.Publish(evt), materializer)
                              .ContinueWith(
                                   t =>
                                   {
                                       if (t.IsFaulted)
                                           Log.Warning(t.Exception, "Event Stream Failed");

                                       Self.Tell(new TryQueryEventSource());
                                   }),
                        e =>
                        {
                            if (e is not TaskCanceledException)
                                Log.Error(e, "Error on get Event Sources from Service");
                            Timers.StartSingleTimer(nameof(TryQueryEventSource), new TryQueryEventSource(), TimeSpan.FromSeconds(30));
                        }));
        }

        private sealed record TryQueryEventSource;
        
    }
}