// The MIT License (MIT)
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Akka.Persistence.Journal;
using Akkatecture.Aggregates;
using Akkatecture.Core;
using Akkatecture.Extensions;
using JetBrains.Annotations;
using Tauron;

namespace Akkatecture.Events;

[PublicAPI]
public abstract class
    AggregateEventUpcaster<TAggregate, TIdentity> : AggregateEventUpcaster<TAggregate, TIdentity,
        IEventUpcaster<TAggregate, TIdentity>>
    where TAggregate : IAggregateRoot<TIdentity>
    where TIdentity : IIdentity { }

public abstract class AggregateEventUpcaster<TAggregate, TIdentity, TEventUpcaster> : IReadEventAdapter,
    IEventUpcaster<TAggregate, TIdentity>
    where TEventUpcaster : class, IEventUpcaster<TAggregate, TIdentity>
    where TAggregate : IAggregateRoot<TIdentity>
    where TIdentity : IIdentity
{
    // ReSharper disable once StaticMemberInGenericType
    private static ConcurrentDictionary<Type, bool> _decisionCache = new();

    private readonly IReadOnlyDictionary<Type, Func<TEventUpcaster, IAggregateEvent, IAggregateEvent>>
        _upcastFunctions;


    protected AggregateEventUpcaster()
    {
        var upcastTypes = GetType().GetAggregateEventUpcastTypes();
        var dictionary = upcastTypes.ToDictionary(type => type, _ => true);
        _decisionCache = new ConcurrentDictionary<Type, bool>(dictionary);

        if(this is not TEventUpcaster)
            throw new InvalidOperationException(
                $"Event applier of type '{GetType().PrettyPrint()}' has a wrong generic argument '{typeof(TEventUpcaster).PrettyPrint()}'");

        _upcastFunctions = GetType().GetAggregateEventUpcastMethods<TAggregate, TIdentity, TEventUpcaster>();
    }


    public IAggregateEvent<TAggregate, TIdentity>? Upcast(IAggregateEvent<TAggregate, TIdentity> aggregateEvent)
    {
        Type aggregateEventType = aggregateEvent.GetType();

        if(!_upcastFunctions.TryGetValue(aggregateEventType, out var upcaster))
            throw new ArgumentException($"Upcaster not Found for {aggregateEventType}", nameof(aggregateEvent));

        var evt =
            upcaster((TEventUpcaster)(object)this, aggregateEvent) as IAggregateEvent<TAggregate, TIdentity>;

        return evt;
    }

    public IEventSequence FromJournal(object evt, string manifest)
    {
        if(!ShouldUpcast(evt)) return EventSequence.Single(evt);

        var upcastedEvent =
            Upcast(evt.GetPropertyValue<IAggregateEvent<TAggregate, TIdentity>>("AggregateEvent")) ??
            throw new InvalidOperationException($"Error on Upcasting Event {evt}");

        Type genericType = typeof(CommittedEvent<,,>)
           .MakeGenericType(typeof(TAggregate), typeof(TIdentity), upcastedEvent.GetType());

        object? upcastedCommittedEvent = FastReflection.Shared.FastCreateInstance(
            genericType,
            evt.GetPropertyValue("AggregateIdentity")!,
            upcastedEvent,
            evt.GetPropertyValue("Metadata")!,
            evt.GetPropertyValue("Timestamp")!,
            evt.GetPropertyValue("AggregateSequenceNumber")!);

        return EventSequence.Single(upcastedCommittedEvent);
    }

    private bool ShouldUpcast(object potentialUpcast)
    {
        Type type = potentialUpcast.GetType();

        if(!(potentialUpcast is ICommittedEvent<TAggregate, TIdentity>)) return false;

        Type eventType = type.GenericTypeArguments[2];

        return _decisionCache.ContainsKey(eventType);
    }
}