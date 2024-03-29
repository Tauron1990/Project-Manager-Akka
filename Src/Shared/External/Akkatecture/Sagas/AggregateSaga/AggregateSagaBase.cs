// The MIT License (MIT)
//
// Copyright (c) 2015-2020 Rasmus Mikkelsen
// Copyright (c) 2015-2020 eBay Software Foundation
// Modified from original source https://github.com/eventflow/EventFlow
//
// Copyright (c) 2018 - 2020 Lutando Ngqakaza
// https://github.com/Lutando/Akkatecture
//
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Event;
using Akka.Persistence;
using Akkatecture.Aggregates;
using Akkatecture.Aggregates.Snapshot;
using Akkatecture.Aggregates.Snapshot.Strategies;
using Akkatecture.Core;
using Akkatecture.Events;
using Akkatecture.Extensions;
using Akkatecture.Jobs.Commands;
using Akkatecture.Sagas.SagaTimeouts;
using JetBrains.Annotations;
using Tauron;
using SnapshotMetadata = Akkatecture.Aggregates.Snapshot.SnapshotMetadata;

namespace Akkatecture.Sagas.AggregateSaga;

[PublicAPI]
public abstract class AggregateSagaBase<TAggregateSaga, TIdentity, TSagaState> : ReceivePersistentActor,
    IAggregateSaga<TIdentity>
    where TAggregateSaga : AggregateSagaBase<TAggregateSaga, TIdentity, TSagaState>
    where TIdentity : SagaId<TIdentity>
    where TSagaState : SagaState<TAggregateSaga, TIdentity, IMessageApplier<TAggregateSaga, TIdentity>>
{
    private static readonly IReadOnlyDictionary<Type, Action<TSagaState?, IAggregateEvent?>> ApplyMethodsFromState =
        typeof(TSagaState).GetAggregateStateEventApplyMethods<TAggregateSaga, TIdentity, TSagaState>();

    private static readonly IReadOnlyDictionary<Type, Action<TSagaState?, IAggregateSnapshot?>>
        HydrateMethodsFromState =
            typeof(TSagaState).GetAggregateSnapshotHydrateMethods<TAggregateSaga, TIdentity, TSagaState>();

    private static readonly IAggregateName SagaName = typeof(TAggregateSaga).GetSagaName();

    // ReSharper disable once StaticMemberInGenericType
    private static readonly List<Type> SagaTimeoutTypes = new();
    private CircularBuffer<ISourceId> _previousSourceIds = new(100);

    protected IEventDefinitionService EventDefinitionService;
    protected ISnapshotDefinitionService SnapshotDefinitionService;

    #pragma warning disable MA0051
    protected AggregateSagaBase()
        #pragma warning restore MA0051
    {
        SetReceiveTimeout(TimeSpan.FromHours(1));
        Command<ReceiveTimeout>(_ => Context.Stop(Self));

        Settings = new AggregateSagaSettings(Context.System.Settings.Config);
        string? idValue = Context.Self.Path.Name;
        #pragma warning disable MA0056
        PersistenceId = idValue;
        #pragma warning restore MA0056
        Id = (TIdentity)(FastReflection.Shared.FastCreateInstance(typeof(TIdentity), idValue) ??
                         throw new InvalidOperationException("Identity Creation Failed"));

        if(Id is null)
            throw new InvalidOperationException(
                $"Identity for Saga '{Id?.GetType().PrettyPrint()}' could not be activated.");

        if(this is not TAggregateSaga)
            throw new InvalidOperationException(
                $"AggregateSaga {Name} specifies Type={typeof(TAggregateSaga).PrettyPrint()} as generic argument, it should be its own type.");

        if(State is null)
            try
            {
                State = (TSagaState)(FastReflection.Shared.FastCreateInstance(typeof(TSagaState)) ??
                                     #pragma warning disable EX006
                                     throw new InvalidOperationException("Saga State Creation Failed"));
                #pragma warning restore EX006
            }
            catch (Exception exception)
            {
                Context.GetLogger().Error(
                    exception,
                    "AggregateSaga of Name={1}; was unable to activate SagaState of Type={0}.",
                    Name,
                    typeof(TSagaState).PrettyPrint());
            }

        if(Settings.AutoReceive)
        {
            InitReceives();
            InitAsyncReceives();
        }

        InitTimeoutJobManagers();
        InitAsyncTimeoutJobManagers();

        if(Settings.UseDefaultEventRecover)
        {
            Recover<ICommittedEvent<TAggregateSaga, TIdentity, IAggregateEvent<TAggregateSaga, TIdentity>>>(
                Recover);
            Recover<RecoveryCompleted>(Recover);
        }

        if(Settings.UseDefaultSnapshotRecover)
            Recover<SnapshotOffer>(Recover);

        Command<SaveSnapshotSuccess>(SnapshotStatus);
        Command<SaveSnapshotFailure>(SnapshotStatus);

        EventDefinitionService = new EventDefinitionService(Context.GetLogger());
        SnapshotDefinitionService = new SnapshotDefinitionService(Context.GetLogger());
    }

    private Dictionary<Type, IActorRef>? SagaTimeoutManagers { get; set; }

    protected ISnapshotStrategy SnapshotStrategy { get; set; } = SnapshotNeverStrategy.Instance;
    public TSagaState? State { get; }
    public override string PersistenceId { get; }
    public override Recovery Recovery => new(SnapshotSelectionCriteria.Latest);
    public AggregateSagaSettings Settings { get; }

    public IAggregateName Name => SagaName;
    public TIdentity Id { get; }
    public long Version { get; protected set; }
    public bool IsNew => Version <= 0;

    public bool HasSourceId(ISourceId sourceId)
        => !sourceId.IsNone() && _previousSourceIds.Any(id => string.Equals(id.Value, sourceId.Value, StringComparison.Ordinal));

    public IIdentity GetIdentity() => Id;

    protected override void PreStart()
    {
        base.PreStart();
        SagaTimeoutManagers = new Dictionary<Type, IActorRef>();
        foreach (Type sagaTimeoutType in SagaTimeoutTypes)
        {
            Type sagaTimeoutManagerType = typeof(SagaTimeoutManager<>).MakeGenericType(sagaTimeoutType);
            IActorRef? sagaTimeoutManager = Context.ActorOf(
                Props.Create(() => Activator.CreateInstance(sagaTimeoutManagerType) as ActorBase),
                $"{sagaTimeoutType.Name}-timeoutmanager");
            SagaTimeoutManagers.Add(sagaTimeoutType, sagaTimeoutManager);
        }
    }

    public void InitTimeoutJobManagers()
    {
        Type type = GetType();
        var timeoutSubscriptionTypes = type.GetSagaTimeoutSubscriptionTypes();

        if(timeoutSubscriptionTypes.Count == 0) return;

        var methods = type
           .GetTypeInfo()
           .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
           .Where(
                mi =>
                {
                    if(!string.Equals(mi.Name, "HandleTimeout", StringComparison.Ordinal))
                        return false;

                    var parameters = mi.GetParameters();

                    return
                        parameters.Length == 1;
                }).ToDictionary(
                mi => mi.GetParameters()[0].ParameterType,
                mi => mi);

        MethodInfo method = type
           .GetBaseType("ReceivePersistentActor")
           .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
           .Where(
                mi =>
                {
                    if(!string.Equals(mi.Name, "Command", StringComparison.Ordinal)) return false;

                    var parameters = mi.GetParameters();

                    return
                        parameters.Length == 1
                     && parameters[0].ParameterType.Name.Contains("Func", StringComparison.Ordinal);
                })
           .First();

        foreach (Type timeoutSubscriptionType in timeoutSubscriptionTypes)
        {
            Type funcType = typeof(Func<,>).MakeGenericType(timeoutSubscriptionType, typeof(bool));
            var timeoutHandlerFunction = Delegate.CreateDelegate(funcType, this, methods[timeoutSubscriptionType]);
            MethodInfo timeoutHandlerMethod = method.MakeGenericMethod(timeoutSubscriptionType);
            timeoutHandlerMethod.InvokeFast(this, timeoutHandlerFunction);
            SagaTimeoutTypes.Add(timeoutSubscriptionType);
        }
    }

    public void InitAsyncTimeoutJobManagers()
    {
        Type type = GetType();
        var timeoutSubscriptionTypes = type.GetAsyncSagaTimeoutSubscriptionTypes();

        if(timeoutSubscriptionTypes.Count == 0) return;

        var methods = type
           .GetTypeInfo()
           .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
           .Where(
                mi =>
                {
                    if(!string.Equals(mi.Name, "HandleTimeoutAsync", StringComparison.Ordinal))
                        return false;

                    var parameters = mi.GetParameters();

                    return
                        parameters.Length == 1;
                }).ToDictionary(
                mi => mi.GetParameters()[0].ParameterType,
                mi => mi);

        MethodInfo method = type
           .GetBaseType("ReceivePersistentActor")
           .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
           .Where(
                mi =>
                {
                    if(!string.Equals(mi.Name, "CommandAsync", StringComparison.Ordinal)) return false;

                    var parameters = mi.GetParameters();

                    return
                        parameters.Length == 2
                     && parameters[0].ParameterType.Name.Contains("Func", StringComparison.Ordinal);
                })
           .First();

        foreach (Type timeoutSubscriptionType in timeoutSubscriptionTypes)
        {
            Type funcType = typeof(Func<,>).MakeGenericType(timeoutSubscriptionType, typeof(Task));
            var timeoutHandlerFunction = Delegate.CreateDelegate(funcType, this, methods[timeoutSubscriptionType]);
            MethodInfo timeoutHandlerMethod = method.MakeGenericMethod(timeoutSubscriptionType);
            timeoutHandlerMethod.InvokeFast(this, timeoutHandlerFunction, null);

            SagaTimeoutTypes.Add(timeoutSubscriptionType);
        }
    }

    public void InitReceives()
    {
        Type type = GetType();

        var subscriptionTypes =
            type
               .GetSagaEventSubscriptionTypes();

        var methods = type
           .GetTypeInfo()
           .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
           .Where(
                mi =>
                {
                    if(!string.Equals(mi.Name, "Handle", StringComparison.Ordinal) && !string.Equals(mi.Name, "HandleTimeout", StringComparison.Ordinal))
                        return false;

                    var parameters = mi.GetParameters();

                    return
                        parameters.Length == 1;
                })
           .ToDictionary(
                mi => mi.GetParameters()[0].ParameterType,
                mi => mi);

        MethodInfo method = type
           .GetBaseType("ReceivePersistentActor")
           .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
           .Where(
                mi =>
                {
                    if(!string.Equals(mi.Name, "Command", StringComparison.Ordinal)) return false;

                    var parameters = mi.GetParameters();

                    return
                        parameters.Length == 1
                     && parameters[0].ParameterType.Name.Contains("Func", StringComparison.Ordinal);
                })
           .First();

        foreach (Type subscriptionType in subscriptionTypes)
        {
            Type funcType = typeof(Func<,>).MakeGenericType(subscriptionType, typeof(bool));
            var subscriptionFunction = Delegate.CreateDelegate(funcType, this, methods[subscriptionType]);
            MethodInfo actorReceiveMethod = method.MakeGenericMethod(subscriptionType);

            actorReceiveMethod.InvokeFast(this, subscriptionFunction);
        }
    }

    public void InitAsyncReceives()
    {
        Type type = GetType();

        var subscriptionTypes =
            type
               .GetAsyncSagaEventSubscriptionTypes();

        var methods = type
           .GetTypeInfo()
           .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
           .Where(
                mi =>
                {
                    if(!string.Equals(mi.Name, "HandleAsync", StringComparison.Ordinal) && !string.Equals(mi.Name, "HandleTimeoutAsync", StringComparison.Ordinal))
                        return false;

                    var parameters = mi.GetParameters();

                    return
                        parameters.Length == 1;
                })
           .ToDictionary(
                mi => mi.GetParameters()[0].ParameterType,
                mi => mi);

        MethodInfo method = type
           .GetBaseType("ReceivePersistentActor")
           .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
           .Where(
                mi =>
                {
                    if(!string.Equals(mi.Name, "CommandAsync", StringComparison.Ordinal)) return false;

                    var parameters = mi.GetParameters();

                    return
                        parameters.Length == 2
                     && parameters[0].ParameterType.Name.Contains("Func", StringComparison.Ordinal);
                })
           .First();

        foreach (Type subscriptionType in subscriptionTypes)
        {
            Type funcType = typeof(Func<,>).MakeGenericType(subscriptionType, typeof(Task));
            var subscriptionFunction = Delegate.CreateDelegate(funcType, this, methods[subscriptionType]);
            MethodInfo actorReceiveMethod = method.MakeGenericMethod(subscriptionType);

            actorReceiveMethod.InvokeFast(this, subscriptionFunction, null);
        }
    }

    protected virtual void Emit<TAggregateEvent>(TAggregateEvent aggregateEvent, IMetadata? metadata = null)
        where TAggregateEvent : class, IAggregateEvent<TAggregateSaga, TIdentity>
    {
        var committedEvent = From(aggregateEvent, Version, metadata);
        Persist(committedEvent, ApplyCommittedEvent);
    }

    public virtual void EmitAll(params IAggregateEvent<TAggregateSaga, TIdentity>[] aggregateEvents)
    {
        long version = Version;

        var committedEvents = new List<object>();
        foreach (var aggregateEvent in aggregateEvents)
        {
            object committedEvent = FromObject(aggregateEvent, version + 1);
            committedEvents.Add(committedEvent);
            version++;
        }

        PersistAll(committedEvents, ApplyObjectCommittedEvent);
    }

    protected virtual object FromObject(object aggregateEvent, long version, IMetadata? metadata = null)
    {
        if(aggregateEvent is IAggregateEvent)
        {
            EventDefinitionService.Load(aggregateEvent.GetType());
            EventDefinition eventDefinition = EventDefinitionService.GetDefinition(aggregateEvent.GetType());
            long aggregateSequenceNumber = version + 1;
            var eventId = EventId.NewDeterministic(
                GuidFactories.Deterministic.Namespaces.Events,
                $"{Id.Value}-v{aggregateSequenceNumber}");
            DateTimeOffset now = DateTimeOffset.UtcNow;
            var eventMetadata = new Metadata
                                {
                                    Timestamp = now,
                                    AggregateSequenceNumber = aggregateSequenceNumber,
                                    AggregateName = Name.Value,
                                    AggregateId = Id.Value,
                                    EventId = eventId,
                                    EventName = eventDefinition.Name,
                                    EventVersion = eventDefinition.Version,
                                };
            eventMetadata.Add(MetadataKeys.TimestampEpoch, now.ToUnixTime().ToString(NumberFormatInfo.InvariantInfo));
            if(metadata != null) eventMetadata.AddRange(metadata);
            Type genericType = typeof(CommittedEvent<,,>)
               .MakeGenericType(typeof(TAggregateSaga), typeof(TIdentity), aggregateEvent.GetType());


            object committedEvent = FastReflection.Shared.FastCreateInstance(
                genericType,
                Id,
                aggregateEvent,
                eventMetadata,
                now,
                aggregateSequenceNumber)!;

            return committedEvent;
        }

        throw new InvalidOperationException("could not perform the required mapping for committed event.");
    }

    private void ApplyObjectCommittedEvent(object committedEvent)
    {
        try
        {
            MethodInfo method = GetType()
               .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
               .Where(info => info.IsFamily || info.IsPublic)
               .Single(info => info.Name.Equals("ApplyCommittedEvent"));

            MethodInfo genericMethod = method.MakeGenericMethod(committedEvent.GetType().GenericTypeArguments[2]);

            genericMethod.InvokeFast(this, committedEvent);
        }
        catch (Exception exception)
        {
            Log.Error(
                exception,
                "Aggregate of Name={0}, and Id={1}; tried to invoke Method={2} with object Type={3} .",
                Name,
                Id,
                nameof(ApplyCommittedEvent),
                committedEvent.GetType().PrettyPrint());
        }
    }

    public virtual CommittedEvent<TAggregateSaga, TIdentity, TAggregateEvent> From<TAggregateEvent>(
        TAggregateEvent aggregateEvent,
        long version, IMetadata? metadata = null)
        where TAggregateEvent : class, IAggregateEvent<TAggregateSaga, TIdentity>
    {
        if(aggregateEvent is null) throw new ArgumentNullException(nameof(aggregateEvent));

        EventDefinitionService.Load(aggregateEvent.GetType());
        EventDefinition eventDefinition = EventDefinitionService.GetDefinition(aggregateEvent.GetType());
        long aggregateSequenceNumber = version + 1;
        var eventId = EventId.NewDeterministic(
            GuidFactories.Deterministic.Namespaces.Events,
            $"{Id.Value}-v{aggregateSequenceNumber}");
        DateTimeOffset now = DateTimeOffset.UtcNow;
        var eventMetadata = new Metadata
                            {
                                Timestamp = now,
                                AggregateSequenceNumber = aggregateSequenceNumber,
                                AggregateName = Name.Value,
                                AggregateId = Id.Value,
                                EventId = eventId,
                                EventName = eventDefinition.Name,
                                EventVersion = eventDefinition.Version,
                            };
        eventMetadata.Add(MetadataKeys.TimestampEpoch, now.ToUnixTime().ToString(CultureInfo.InvariantCulture));
        if(metadata != null) eventMetadata.AddRange(metadata);

        var committedEvent =
            new CommittedEvent<TAggregateSaga, TIdentity, TAggregateEvent>(
                Id,
                aggregateEvent,
                eventMetadata,
                now,
                aggregateSequenceNumber);

        return committedEvent;
    }

    protected virtual IAggregateSnapshot<TAggregateSaga, TIdentity>? CreateSnapshot()
    {
        Log.Info(
            "AggregateSaga of Name={0}, and Id={2}; attempted to create a snapshot, override the {2}() method to get snapshotting to function.",
            Name,
            Id,
            nameof(CreateSnapshot));

        return null;
    }

    public virtual void RequestTimeout<TTimeout>(TTimeout timeoutMessage, TimeSpan timeSpan)
        where TTimeout : class, ISagaTimeoutJob
    {
        Type timeoutMessageType = timeoutMessage.GetType();
        IActorRef? manager = SagaTimeoutManagers?[timeoutMessageType];
        SagaTimeoutId sagaTimeoutId = SagaTimeoutId.New;
        var scheduledMessage = new Schedule<TTimeout, SagaTimeoutId>(
            sagaTimeoutId,
            timeoutMessage,
            DateTime.UtcNow.Add(timeSpan));
        manager.Tell(scheduledMessage);
    }

    #pragma warning disable AV1551
    public void RequestTimeout<TTimeout>(TimeSpan timeSpan) where TTimeout : class, ISagaTimeoutJob, new()
    #pragma warning restore AV1551
    {
        RequestTimeout(new TTimeout(), timeSpan);
    }

    protected void ApplyCommittedEvent<TAggregateEvent>(
        ICommittedEvent<TAggregateSaga, TIdentity, TAggregateEvent> committedEvent)
        where TAggregateEvent : class, IAggregateEvent<TAggregateSaga, TIdentity>
    {
        var applyMethods = GetEventApplyMethods(committedEvent.AggregateEvent);
        applyMethods(committedEvent.AggregateEvent);

        Log.Info(
            "AggregateSaga of Name={0}, and Id={1}; committed and applied an AggregateEvent of Type={2}",
            Name,
            Id,
            typeof(TAggregateEvent).PrettyPrint());

        Version++;

        var domainEvent = new DomainEvent<TAggregateSaga, TIdentity, TAggregateEvent>(
            Id,
            committedEvent.AggregateEvent,
            committedEvent.Metadata,
            committedEvent.Timestamp,
            Version);

        Publish(domainEvent);

        if(!SnapshotStrategy.ShouldCreateSnapshot(this)) return;

        var aggregateSnapshot = CreateSnapshot();

        if(aggregateSnapshot is null) return;

        SnapshotDefinitionService.Load(aggregateSnapshot.GetType());
        SnapshotDefinition snapshotDefinition = SnapshotDefinitionService.GetDefinition(aggregateSnapshot.GetType());
        var snapshotMetadata = new SnapshotMetadata
                               {
                                   AggregateId = Id.Value,
                                   AggregateName = Name.Value,
                                   AggregateSequenceNumber = Version,
                                   SnapshotName = snapshotDefinition.Name,
                                   SnapshotVersion = snapshotDefinition.Version,
                               };

        var committedSnapshot =
            new CommittedSnapshot<TAggregateSaga, TIdentity, IAggregateSnapshot<TAggregateSaga, TIdentity>>(
                Id,
                aggregateSnapshot,
                snapshotMetadata,
                committedEvent.Timestamp,
                Version);

        SaveSnapshot(committedSnapshot);
    }

    protected virtual void Publish<TEvent>(TEvent aggregateEvent)
    {
        Context.System.EventStream.Publish(aggregateEvent);
        Log.Info(
            "Aggregate of Name={0}, and Id={1}; published DomainEvent of Type={2}.",
            Name,
            Id,
            typeof(TEvent).PrettyPrint());
    }

    protected Action<IAggregateEvent?> GetEventApplyMethods<TAggregateEvent>(TAggregateEvent aggregateEvent)
        where TAggregateEvent : IAggregateEvent<TAggregateSaga, TIdentity>
    {
        Type eventType = aggregateEvent.GetType();

        if(!ApplyMethodsFromState.TryGetValue(eventType, out var applyMethod))
            #pragma warning disable GU0090
            #pragma warning disable MA0025
            throw new NotImplementedException(
                $"SagaState of Type={State?.GetType().PrettyPrint()} does not have an 'Apply' method that takes in an aggregate event of Type={eventType.PrettyPrint()} as an argument.");
        #pragma warning restore MA0025
        #pragma warning restore GU0090

        var aggregateApplyMethod = applyMethod.Bind(State);

        return aggregateApplyMethod;
    }

    protected virtual void ApplyEvent(IAggregateEvent<TAggregateSaga, TIdentity> aggregateEvent)
    {
        var eventApplier = GetEventApplyMethods(aggregateEvent);

        eventApplier(aggregateEvent);

        Version++;
    }

    protected virtual bool Recover(
        ICommittedEvent<TAggregateSaga, TIdentity, IAggregateEvent<TAggregateSaga, TIdentity>> committedEvent)
    {
        try
        {
            Log.Debug(
                "AggregateSaga of Name={0}, Id={1}, and Version={2}, is recovering with CommittedEvent of Type={3}.",
                Name,
                Id,
                Version,
                committedEvent.GetType().PrettyPrint());
            ApplyEvent(committedEvent.AggregateEvent);
        }
        catch (Exception exception)
        {
            Log.Error(
                exception,
                "Aggregate of Name={0}, Id={1}; while recovering with event of Type={2} caused an exception.",
                Name,
                Id,
                committedEvent.GetType().PrettyPrint());

            return false;
        }

        return true;
    }

    protected virtual bool Recover(SnapshotOffer aggregateSnapshotOffer)
    {
        try
        {
            Log.Debug(
                "AggregateSaga of Name={0}, and Id={1}; has received a SnapshotOffer of Type={2}.",
                Name,
                Id,
                aggregateSnapshotOffer.Snapshot.GetType().PrettyPrint());
            var comittedSnapshot =
                aggregateSnapshotOffer.Snapshot as CommittedSnapshot<TAggregateSaga, TIdentity,
                    IAggregateSnapshot<TAggregateSaga, TIdentity>>;
            HydrateSnapshot(comittedSnapshot?.AggregateSnapshot, aggregateSnapshotOffer.Metadata.SequenceNr);
        }
        catch (Exception exception)
        {
            Log.Error(
                exception,
                "AggregateSaga of Name={0}, Id={1}; recovering with snapshot of Type={2} caused an exception.",
                Name,
                Id,
                aggregateSnapshotOffer.Snapshot.GetType().PrettyPrint());

            return false;
        }

        return true;
    }

    protected virtual void HydrateSnapshot(
        IAggregateSnapshot<TAggregateSaga, TIdentity>? aggregateSnapshot,
        long version)
    {
        if(aggregateSnapshot is null) return;

        var snapshotHydrater = GetSnapshotHydrateMethods(aggregateSnapshot);

        snapshotHydrater(aggregateSnapshot);

        Version = version;
    }

    protected Action<IAggregateSnapshot> GetSnapshotHydrateMethods<TAggregateSnapshot>(
        TAggregateSnapshot aggregateEvent)
        where TAggregateSnapshot : IAggregateSnapshot<TAggregateSaga, TIdentity>
    {
        Type snapshotType = aggregateEvent.GetType();

        if(!HydrateMethodsFromState.TryGetValue(snapshotType, out var hydrateMethod))
            #pragma warning disable GU0090
            #pragma warning disable MA0025
            throw new NotImplementedException(
                $"SagaState of Type={State?.GetType().PrettyPrint()} does not have a 'Hydrate' method that takes in an aggregate snapshot of Type={snapshotType.PrettyPrint()} as an argument.");
        #pragma warning restore MA0025
        #pragma warning restore GU0090

        var snapshotHydrateMethod = hydrateMethod.Bind(State);

        return snapshotHydrateMethod;
    }

    protected void SetSourceIdHistory(int count)
    {
        _previousSourceIds = new CircularBuffer<ISourceId>(count);
    }

    protected virtual void SetSnapshotStrategy(ISnapshotStrategy? snapshotStrategy)
    {
        if(snapshotStrategy != null)
            SnapshotStrategy = snapshotStrategy;
    }

    protected virtual bool SnapshotStatus(SaveSnapshotSuccess snapshotSuccess)
    {
        Log.Debug(
            "Aggregate of Name={0}, and Id={1}; saved a snapshot at Version={2}.",
            Name,
            Id,
            snapshotSuccess.Metadata.SequenceNr);
        DeleteSnapshots(new SnapshotSelectionCriteria(snapshotSuccess.Metadata.SequenceNr - 1));

        return true;
    }

    protected virtual bool SnapshotStatus(SaveSnapshotFailure snapshotFailure)
    {
        Log.Error(
            snapshotFailure.Cause,
            "Aggregate of Name={0}, and Id={1}; failed to save snapshot at Version={2}.",
            Name,
            Id,
            snapshotFailure.Metadata.SequenceNr);

        return true;
    }

    protected virtual bool Recover(RecoveryCompleted recoveryCompleted)
    {
        Log.Debug(
            "Aggregate of Name={0}, and Id={1}; has completed recovering from it's event journal at Version={2}.",
            Name,
            Id,
            Version);

        return true;
    }
}