﻿// The MIT License (MIT)
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
using Akka.Actor;
using Akka.Event;
using Akka.Persistence;
using Akkatecture.Aggregates.Snapshot;
using Akkatecture.Aggregates.Snapshot.Strategies;
using Akkatecture.Commands;
using Akkatecture.Core;
using Akkatecture.Events;
using Akkatecture.Extensions;
using JetBrains.Annotations;
using Tauron;
using SnapshotMetadata = Akkatecture.Aggregates.Snapshot.SnapshotMetadata;

namespace Akkatecture.Aggregates;

[PublicAPI]
public abstract class AggregateRoot<TAggregate, TIdentity, TAggregateState> : ReceivePersistentActor,
    IAggregateRoot<TIdentity>
    where TAggregate : AggregateRoot<TAggregate, TIdentity, TAggregateState>
    where TAggregateState : AggregateState<TAggregate, TIdentity, IMessageApplier<TAggregate, TIdentity>>
    where TIdentity : IIdentity
{
    private static readonly IReadOnlyDictionary<Type, Action<TAggregateState, IAggregateEvent>>
        ApplyMethodsFromState = typeof(TAggregateState)
           .GetAggregateStateEventApplyMethods<TAggregate, TIdentity, TAggregateState>()!;

    private static readonly IReadOnlyDictionary<Type, Action<TAggregateState, IAggregateSnapshot>>
        HydrateMethodsFromState = typeof(TAggregateState)
           .GetAggregateSnapshotHydrateMethods<TAggregate, TIdentity, TAggregateState>()!;

    private static readonly IAggregateName AggregateName = typeof(TAggregate).GetAggregateName();
    private readonly IEventDefinitionService _eventDefinitionService;
    private readonly ISnapshotDefinitionService _snapshotDefinitionService;
    private CircularBuffer<ISourceId> _previousSourceIds = new(100);

    protected AggregateRoot(TIdentity id, AggregateRootSettings? settings = null)
    {
        Settings = settings ?? new AggregateRootSettings(Context.System.Settings.Config);

        if(id is null)
            throw new ArgumentNullException(nameof(id));

        if(this is not TAggregate)
            throw new InvalidOperationException(
                $"Aggregate {Name} specifies Type={typeof(TAggregate).PrettyPrint()} as generic argument, it should be its own type.");

        if(State is null)
            try
            {
                State = (TAggregateState?)FastReflection.Shared.FastCreateInstance(typeof(TAggregateState));
            }
            catch (Exception exception)
            {
                Context.GetLogger().Error(
                    exception,
                    "Unable to activate AggregateState of Type={0} for AggregateRoot of Name={1}.",
                    typeof(TAggregateState).PrettyPrint(),
                    Name);
            }

        PinnedCommand = null!;
        _eventDefinitionService = new EventDefinitionService(Context.GetLogger());
        _snapshotDefinitionService = new SnapshotDefinitionService(Context.GetLogger());
        Id = id;
        #pragma warning disable MA0056
        PersistenceId = id.Value;
        #pragma warning restore MA0056
        SetSourceIdHistory(100);

        if(Settings.UseDefaultSnapshotRecover) Recover<SnapshotOffer>(Recover);

        Command<SaveSnapshotSuccess>(SnapshotStatus);
        Command<SaveSnapshotFailure>(SnapshotStatus);

        if(Settings.UseDefaultEventRecover)
        {
            Recover<ICommittedEvent<TAggregate, TIdentity, IAggregateEvent<TAggregate, TIdentity>>>(Recover);
            Recover<RecoveryCompleted>(Recover);
        }

        InitReceives();
        SetReceiveTimeout(Settings.SetReceiveTimeout);
        Command<ReceiveTimeout>(Timeout);
    }

    private ICommand<TAggregate, TIdentity> PinnedCommand { get; set; }
    private object? PinnedReply { get; set; }
    private ISnapshotStrategy SnapshotStrategy { get; set; } = SnapshotNeverStrategy.Instance;
    public TAggregateState? State { get; }

    public override string PersistenceId { get; }

    public override Recovery Recovery => new(SnapshotSelectionCriteria.Latest);
    private AggregateRootSettings Settings { get; }

    public IAggregateName Name => AggregateName;
    public TIdentity Id { get; }
    public long Version { get; protected set; }
    public bool IsNew => Version <= 0;

    public bool HasSourceId(ISourceId sourceId)
        => !sourceId.IsNone() && _previousSourceIds.Any(id => string.Equals(id.Value, sourceId.Value, StringComparison.Ordinal));

    public IIdentity GetIdentity() => Id;


    protected void SetSourceIdHistory(int count)
        => _previousSourceIds = new CircularBuffer<ISourceId>(count);

    public virtual void Emit<TAggregateEvent>(TAggregateEvent aggregateEvent, IMetadata? metadata = null)
        where TAggregateEvent : class, IAggregateEvent<TAggregate, TIdentity>
    {
        var committedEvent = From(aggregateEvent, Version, metadata);
        Persist(committedEvent, ApplyCommittedEvent);
    }

    public virtual void EmitAll(params IAggregateEvent<TAggregate, TIdentity>[] aggregateEvents)
        => EmitAll(aggregateEvents.AsEnumerable());

    public virtual void EmitAll(IEnumerable<IAggregateEvent<TAggregate, TIdentity>> aggregateEvents)
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
            _eventDefinitionService.Load(aggregateEvent.GetType());
            EventDefinition eventDefinition = _eventDefinitionService.GetDefinition(aggregateEvent.GetType());
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
                                    SourceId = PinnedCommand.SourceId,
                                    EventId = eventId,
                                    EventName = eventDefinition.Name,
                                    EventVersion = eventDefinition.Version,
                                };
            eventMetadata.Add(MetadataKeys.TimestampEpoch, now.ToUnixTime().ToString(CultureInfo.InvariantCulture));
            if(metadata != null) eventMetadata.AddRange(metadata);
            Type genericType = typeof(CommittedEvent<,,>)
               .MakeGenericType(typeof(TAggregate), typeof(TIdentity), aggregateEvent.GetType());

            object? committedEvent = FastReflection.Shared.FastCreateInstance(
                genericType,
                Id,
                aggregateEvent,
                eventMetadata,
                now,
                aggregateSequenceNumber);

            return committedEvent ??
                   throw new InvalidOperationException($"No Commited Event Created {aggregateEvent} ");
        }

        throw new InvalidOperationException("could not perform the required mapping for committed event.");
    }

    public virtual CommittedEvent<TAggregate, TIdentity, TAggregateEvent> From<TAggregateEvent>(
        TAggregateEvent aggregateEvent,
        long version, IMetadata? metadata = null)
        where TAggregateEvent : class, IAggregateEvent<TAggregate, TIdentity>
    {
        if(aggregateEvent is null) throw new ArgumentNullException(nameof(aggregateEvent));

        _eventDefinitionService.Load(aggregateEvent.GetType());
        EventDefinition eventDefinition = _eventDefinitionService.GetDefinition(aggregateEvent.GetType());
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
                                SourceId = PinnedCommand.SourceId,
                                EventId = eventId,
                                EventName = eventDefinition.Name,
                                EventVersion = eventDefinition.Version,
                            };
        eventMetadata.Add(MetadataKeys.TimestampEpoch, now.ToUnixTime().ToString(CultureInfo.InvariantCulture));
        if(metadata != null) eventMetadata.AddRange(metadata);

        var committedEvent = new CommittedEvent<TAggregate, TIdentity, TAggregateEvent>(
            Id,
            aggregateEvent,
            eventMetadata,
            now,
            aggregateSequenceNumber);

        return committedEvent;
    }

    protected virtual IAggregateSnapshot<TAggregate, TIdentity>? CreateSnapshot()
    {
        Log.Warning(
            "Aggregate of Name={0}, and Id={1}; attempted to create a snapshot, override the {2}() method to get snapshotting to function.",
            Name,
            Id,
            nameof(CreateSnapshot));

        return null;
    }

    protected void ApplyCommittedEvent<TAggregateEvent>(
        ICommittedEvent<TAggregate, TIdentity, TAggregateEvent> committedEvent)
        where TAggregateEvent : class, IAggregateEvent<TAggregate, TIdentity>
    {
        var applyMethods = GetEventApplyMethods(committedEvent.AggregateEvent);
        applyMethods(committedEvent.AggregateEvent);

        Log.Info(
            "Aggregate of Name={0}, and Id={1}; committed and applied an AggregateEvent of Type={2}.",
            Name,
            Id,
            typeof(TAggregateEvent).PrettyPrint());

        Version++;

        var domainEvent = new DomainEvent<TAggregate, TIdentity, TAggregateEvent>(
            Id,
            committedEvent.AggregateEvent,
            committedEvent.Metadata,
            committedEvent.Timestamp,
            Version);

        Publish(domainEvent);
        ReplyIfAvailable();

        if(!SnapshotStrategy.ShouldCreateSnapshot(this)) return;

        var aggregateSnapshot = CreateSnapshot();

        if(aggregateSnapshot is null) return;

        _snapshotDefinitionService.Load(aggregateSnapshot.GetType());
        SnapshotDefinition snapshotDefinition = _snapshotDefinitionService.GetDefinition(aggregateSnapshot.GetType());
        var snapshotMetadata = new SnapshotMetadata
                               {
                                   AggregateId = Id.Value,
                                   AggregateName = Name.Value,
                                   AggregateSequenceNumber = Version,
                                   SnapshotName = snapshotDefinition.Name,
                                   SnapshotVersion = snapshotDefinition.Version,
                               };

        var committedSnapshot =
            new CommittedSnapshot<TAggregate, TIdentity, IAggregateSnapshot<TAggregate, TIdentity>>(
                Id,
                aggregateSnapshot,
                snapshotMetadata,
                committedEvent.Timestamp,
                Version);

        SaveSnapshot(committedSnapshot);
    }

    private void ApplyObjectCommittedEvent(object committedEvent)
    {
        try
        {
            MethodInfo method = GetType()
               .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
               .Where(methodInfo => methodInfo.IsFamily || methodInfo.IsPublic)
               .Single(methodInfo => methodInfo.Name.Equals("ApplyCommittedEvent"));

            MethodInfo genericMethod = method.MakeGenericMethod(committedEvent.GetType().GenericTypeArguments[2]);

            genericMethod.InvokeFast(this, committedEvent);
        }
        catch (Exception exception)
        {
            Log.Error(
                exception,
                "Aggregate of Name={0}, and Id={1}; tried to invoke Method={2} with object Type={3}.",
                Name,
                Id,
                nameof(ApplyCommittedEvent),
                committedEvent.GetType().PrettyPrint());
        }
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

    protected override bool AroundReceive(Receive receive, object message)
    {
        if(message is not Command<TAggregate, TIdentity> command) return base.AroundReceive(receive, message);

        if(IsNew || Id.Equals(command.AggregateId))
            PinnedCommand = command;

        return base.AroundReceive(receive, message);
    }

    protected virtual void Reply(object replyMessage)
    {
        if(!Sender.IsNobody()) PinnedReply = replyMessage;
    }

    protected virtual void ReplyFailure(object replyMessage)
    {
        if(!Sender.IsNobody()) Context.Sender.Tell(replyMessage);
    }

    protected virtual void ReplyIfAvailable()
    {
        if(PinnedReply != null)
            Sender.Tell(PinnedReply);

        PinnedReply = null;
        PinnedCommand = null!;
    }

    protected override void Unhandled(object message)
    {
        Log.Warning(
            "Aggregate of Name={0}, and Id={1}; has received an unhandled message of Type={2}.",
            Name,
            Id,
            message.GetType().PrettyPrint());
        base.Unhandled(message);
    }

    protected IEnumerable<IAggregateEvent<TAggregate, TIdentity>> Events(
        params IAggregateEvent<TAggregate, TIdentity>[] events) => events.ToList();

    protected Action<IAggregateEvent> GetEventApplyMethods<TAggregateEvent>(TAggregateEvent aggregateEvent)
        where TAggregateEvent : class, IAggregateEvent<TAggregate, TIdentity>
    {
        Type eventType = aggregateEvent.GetType();

        if(!ApplyMethodsFromState.TryGetValue(eventType, out var applyMethod))
        {
            #pragma warning disable GU0090
            #pragma warning disable MA0025
            throw new NotImplementedException(
                $"AggregateState of Type={State?.GetType().PrettyPrint()} does not have an 'Apply' method that takes in an aggregate event of Type={eventType.PrettyPrint()} as an argument.");
            #pragma warning restore MA0025
            #pragma warning restore GU0090
        }

        var aggregateApplyMethod = applyMethod!.Bind(State);

        return aggregateApplyMethod;
    }

    protected Action<IAggregateSnapshot> GetSnapshotHydrateMethods<TAggregateSnapshot>(
        TAggregateSnapshot aggregateEvent)
        where TAggregateSnapshot : class, IAggregateSnapshot<TAggregate, TIdentity>
    {
        Type snapshotType = aggregateEvent.GetType();

        if(!HydrateMethodsFromState.TryGetValue(snapshotType, out var hydrateMethod))
        {
            #pragma warning disable GU0090
            #pragma warning disable MA0025
            throw new NotImplementedException(
                $"AggregateState of Type={State?.GetType().PrettyPrint()} does not have a 'Hydrate' method that takes in an aggregate snapshot of Type={snapshotType.PrettyPrint()} as an argument.");
            #pragma warning restore MA0025
            #pragma warning restore GU0090
        }


        var snapshotHydrateMethod = hydrateMethod!.Bind(State);

        return snapshotHydrateMethod;
    }

    protected virtual void ApplyEvent(IAggregateEvent<TAggregate, TIdentity> aggregateEvent)
    {
        var eventApplier = GetEventApplyMethods(aggregateEvent);

        eventApplier(aggregateEvent);

        Version++;
    }

    protected virtual void HydrateSnapshot(
        IAggregateSnapshot<TAggregate, TIdentity> aggregateSnapshot,
        long version)
    {
        var snapshotHydrater = GetSnapshotHydrateMethods(aggregateSnapshot);

        snapshotHydrater(aggregateSnapshot);

        Version = version;
    }

    protected virtual bool Recover(
        ICommittedEvent<TAggregate, TIdentity, IAggregateEvent<TAggregate, TIdentity>> committedEvent)
    {
        try
        {
            Log.Debug(
                "Aggregate of Name={0}, Id={1}, and Version={2}, is recovering with CommittedEvent of Type={3}.",
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
                "Aggregate of Name={0}, and Id={1}; has received a SnapshotOffer of Type={2}.",
                Name,
                Id,
                aggregateSnapshotOffer.Snapshot.GetType().PrettyPrint());
            var comittedSnapshot =
                (CommittedSnapshot<TAggregate, TIdentity, IAggregateSnapshot<TAggregate, TIdentity>>)
                aggregateSnapshotOffer.Snapshot;
            HydrateSnapshot(comittedSnapshot.AggregateSnapshot, aggregateSnapshotOffer.Metadata.SequenceNr);
        }
        catch (Exception exception)
        {
            Log.Error(
                exception,
                "Aggregate of Name={0}, Id={1}; recovering with snapshot of Type={2} caused an exception.",
                Name,
                Id,
                aggregateSnapshotOffer.Snapshot.GetType().PrettyPrint());

            return false;
        }

        return true;
    }

    protected virtual void SetSnapshotStrategy(ISnapshotStrategy snapshotStrategy)
    {
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

    public override string ToString() => $"{GetType().PrettyPrint()} v{Version}";

    public virtual bool Timeout(ReceiveTimeout message)
    {
        Log.Debug("Aggregate of Name={0}, and Id={1}; has received a timeout message and will stop.", Name, Id);
        Context.Stop(Self);

        return true;
    }

    public override void AroundPreRestart(Exception cause, object message)
    {
        Log.Error(
            cause,
            "Aggregate of Name={0}, and Id={1}; has experienced an error and will now restart",
            Name,
            Id);
        base.AroundPreRestart(cause, message);
    }


    protected void Command<TCommand, TCommandHandler>(Predicate<TCommand>? shouldHandle = null)
        where TCommand : ICommand<TAggregate, TIdentity>
        where TCommandHandler : CommandHandler<TAggregate, TIdentity, TCommand>
    {
        try
        {
            if(FastReflection.Shared.FastCreateInstance(typeof(TCommandHandler)) is not TCommandHandler handler)
                Log.Error("Unable to resolve CommandHandler of Type={0} for Aggregate of Type={1}.");
            else
                Command(command => handler.HandleCommand((TAggregate)this, Context, command), shouldHandle);
        }
        catch (Exception exception)
        {
            Log.Error(
                exception,
                "Unable to activate CommandHandler of Type={0} for Aggregate of Type={1}.",
                typeof(TCommandHandler).PrettyPrint(),
                typeof(TAggregate).PrettyPrint());
        }
    }

    public void InitReceives()
    {
        Type type = GetType();

        var subscriptionTypes =
            type
               .GetAggregateExecuteTypes();

        var methods = type
           .GetTypeInfo()
           .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
           .Where(
                mi =>
                {
                    if(!string.Equals(mi.Name, "Execute", StringComparison.Ordinal)) return false;

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

            actorReceiveMethod.Invoke(this, new object[] { subscriptionFunction });
        }
    }
}