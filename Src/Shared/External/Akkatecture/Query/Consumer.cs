using System.Linq;
using Akka;
using Akka.Actor;
using Akka.Configuration;
using Akka.Persistence.Query;
using Akka.Streams.Dsl;
using Akkatecture.Aggregates;
using Akkatecture.Events;
using Akkatecture.Extensions;
using JetBrains.Annotations;

namespace Akkatecture.Query;

[PublicAPI]
public class Consumer
{
    internal Consumer(
        string name,
        ActorSystem actorSystem)
    {
        ActorSystem = actorSystem;
        Name = name;
    }

    private Consumer(
        string name,
        Config config)
    {
        var actorSystem = ActorSystem.Create(name, config);
        ActorSystem = actorSystem;
        Name = name;
    }

    public ActorSystem ActorSystem { get; set; }
    protected string Name { get; set; }

    public static Consumer Create(string name, Config config) => new(name, config);

    #pragma warning disable AV1551
    public static Consumer Create(ActorSystem actorSystem) => new(actorSystem.Name, actorSystem);
    #pragma warning restore AV1551

    public Consumer<TJournal> Using<TJournal>(
        string readJournalPluginId)
        where TJournal : IEventsByTagQuery, ICurrentEventsByTagQuery
    {
        PersistenceQuery? persistenceQuery = PersistenceQuery.Get(ActorSystem);
        var readJournal = persistenceQuery.ReadJournalFor<TJournal>(readJournalPluginId);

        return new Consumer<TJournal>(Name, ActorSystem, readJournal);
    }
}

[PublicAPI]
public class Consumer<TJournal> : Consumer
    where TJournal : IEventsByTagQuery, ICurrentEventsByTagQuery
{
    public Consumer(
        string name,
        ActorSystem actorSystem,
        TJournal journal)
        : base(name, actorSystem)
        => Journal = journal;

    protected TJournal Journal { get; }

    public Source<EventEnvelope, NotUsed> EventsFromAggregate<TAggregate>(Offset? offset = null)
        where TAggregate : IAggregateRoot
    {
        var mapper = new DomainEventReadAdapter();
        AggregateName aggregateName = typeof(TAggregate).GetAggregateName();

        return Journal
           .EventsByTag(aggregateName.Value, offset)
           .Select(
                envelope =>
                {
                    object? domainEvent = mapper.FromJournal(envelope.Event, string.Empty).Events.Single();

                    return new EventEnvelope(envelope.Offset, envelope.PersistenceId, 
                        envelope.SequenceNr, domainEvent, envelope.Timestamp, envelope.Tags);
                });
    }

    public Source<EventEnvelope, NotUsed> CurrentEventsFromAggregate<TAggregate>(Offset? offset = null)
        where TAggregate : IAggregateRoot
    {
        var mapper = new DomainEventReadAdapter();
        AggregateName aggregateName = typeof(TAggregate).GetAggregateName();

        return Journal
           .CurrentEventsByTag(aggregateName.Value, offset)
           .Select(
                envelope =>
                {
                    object? domainEvent = mapper.FromJournal(envelope.Event, string.Empty).Events.Single();

                    return new EventEnvelope(envelope.Offset, envelope.PersistenceId, envelope.SequenceNr,
                        domainEvent, envelope.Timestamp, envelope.Tags);
                });
    }
}