using Akka.Actor;
using Akka.DependencyInjection;
using Akka.Persistence.MongoDb.Query;
using Akkatecture.Aggregates;
using Akkatecture.Core;
using LiquidProjections;
using Tauron.Akkatecture.Projections;

namespace SimpleProjectManager.Server.Core.Projections.Core
{
    public static class ProjectionBuilder
    {
        private sealed class EventMapDispatcher : IEventMap<ProjectionContext>
        {
            private readonly DomainEventDispatcher _eventDispatcher;
            private readonly IEventMap<ProjectionContext> _originalMap;

            public EventMapDispatcher(DomainEventDispatcher eventDispatcher, IEventMap<ProjectionContext> originalMap)
            {
                _eventDispatcher = eventDispatcher;
                _originalMap = originalMap;
            }

            public async Task<bool> Handle(object anEvent, ProjectionContext context)
            {
                var result = await _originalMap.Handle(anEvent, context);

                if(result)
                    _eventDispatcher.Publish((IDomainEvent)anEvent);

                return result;
            }
        }

        public static DomainDispatcher<TProjection, TIdentity> CreateProjection<TProjection, TAggregate, TIdentity>(
            ActorSystem system, Action<DomainEventMapBuilder<TProjection, TAggregate, TIdentity>> mapBuilderAction) 
            where TProjection : class, IProjectorData<TIdentity> 
            where TAggregate : IAggregateRoot<TIdentity>
            where TIdentity : IIdentity
        {
            var resolver = DependencyResolver.For(system);

            var repository = resolver.Resolver.GetService<InternalDataRepository>();
            var eventDispatcher = resolver.Resolver.GetService<DomainEventDispatcher>();

            var evtBuilder = new DomainEventMapBuilder<TProjection, TAggregate, TIdentity>();

            mapBuilderAction(evtBuilder);
            var eventMap = new EventMapDispatcher(eventDispatcher, evtBuilder.Build(new RepositoryProjectorMap<TProjection, TIdentity>(repository)));

            var projector = new DomainProjector(eventMap);

            var dispatcher = new DomainDispatcher<TProjection, TIdentity>(
                new AggregateEventReader<MongoDbReadJournal>(system, "akka.persistence.journal.mongoRead"), projector, repository);

            return dispatcher;
        }
    }
}
