using Akka.Actor;
using Akka.DependencyInjection;
using Akka.Persistence.Query;
using Akkatecture.Aggregates;
using Akkatecture.Core;
using LiquidProjections;
using SimpleProjectManager.Server.Data;
using Tauron.Akkatecture.Projections;

namespace SimpleProjectManager.Server.Core.Projections.Core;

public static class ProjectionBuilder
{
    public static DomainDispatcher<TProjection, TIdentity> CreateProjection<TProjection, TAggregate, TIdentity>(
        ActorSystem system, Action<DomainEventMapBuilder<TProjection, TAggregate, TIdentity>> mapBuilderAction, ILoggerFactory loggerFactory)
        where TProjection : class, IProjectorData<TIdentity>
        where TAggregate : IAggregateRoot<TIdentity>
        where TIdentity : IIdentity
    {
        DependencyResolver? resolver = DependencyResolver.For(system);

        var repository = resolver.Resolver.GetService<IInternalDataRepository>();
        var eventDispatcher = resolver.Resolver.GetService<DomainEventDispatcher>();

        var evtBuilder = new DomainEventMapBuilder<TProjection, TAggregate, TIdentity>();

        mapBuilderAction(evtBuilder);
        var eventMap = new EventMapDispatcher(eventDispatcher, evtBuilder.Build(new RepositoryProjectorMap<TProjection, TIdentity>(repository)));

        var projector = new DomainProjector(eventMap);

        var journalType = Type.GetType(system.Settings.Config.GetString("akka.persistence.journal.queryConfig.class"));

        if(journalType is null || Activator.CreateInstance(journalType, system, system.Settings.Config) is not IReadJournalProvider journalProvider)
            throw new InvalidOperationException("Journal Type not found");


        journalType = journalProvider.GetReadJournal().GetType();

        if(typeof(AggregateEventReader<>)
              .MakeGenericType(journalType)
              .GetConstructor(new[] { typeof(ActorSystem), typeof(string) })
             ?.Invoke(new object[] { system, "akka.persistence.journal.queryConfig" }) is not AggregateEventReader reader)
            throw new InvalidOperationException("Journal Creation Failed");

        var dispatcher = new DomainDispatcher<TProjection, TIdentity>(reader, projector, repository, loggerFactory.CreateLogger<DomainDispatcher<TProjection, TIdentity>>());

        return dispatcher;
    }

    private sealed class EventMapDispatcher : IEventMap<ProjectionContext>
    {
        private readonly DomainEventDispatcher _eventDispatcher;
        private readonly IEventMap<ProjectionContext> _originalMap;

        internal EventMapDispatcher(DomainEventDispatcher eventDispatcher, IEventMap<ProjectionContext> originalMap)
        {
            _eventDispatcher = eventDispatcher;
            _originalMap = originalMap;
        }

        public async Task<bool> Handle(object anEvent, ProjectionContext context)
        {
            bool result = await _originalMap.Handle(anEvent, context).ConfigureAwait(false);

            if(result)
                _eventDispatcher.Publish((IDomainEvent)anEvent);

            return result;
        }
    }
}