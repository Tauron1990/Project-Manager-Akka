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
using Akkatecture.Core;
using Akkatecture.Extensions;

namespace Akkatecture.Aggregates;

public class
    DomainEvent<TAggregate, TIdentity, TAggregateEvent> : IDomainEvent<TAggregate, TIdentity, TAggregateEvent>
    where TAggregate : IAggregateRoot<TIdentity>
    where TIdentity : IIdentity
    where TAggregateEvent : class, IAggregateEvent<TAggregate, TIdentity>
{
    public DomainEvent(
        TIdentity aggregateIdentity,
        TAggregateEvent aggregateEvent,
        Metadata metadata,
        DateTimeOffset timestamp,
        long aggregateSequenceNumber)
    {
        if(timestamp == default) throw new ArgumentNullException(nameof(timestamp));
        if(aggregateIdentity == null || string.IsNullOrEmpty(aggregateIdentity.Value))
            throw new ArgumentNullException(nameof(aggregateIdentity));
        if(aggregateSequenceNumber <= 0) throw new ArgumentOutOfRangeException(nameof(aggregateSequenceNumber));

        AggregateEvent = aggregateEvent ?? throw new ArgumentNullException(nameof(aggregateEvent));
        Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
        Timestamp = timestamp;
        AggregateIdentity = aggregateIdentity;
        AggregateSequenceNumber = aggregateSequenceNumber;
    }

    public Type AggregateType => typeof(TAggregate);
    public Type IdentityType => typeof(TIdentity);
    public Type EventType => typeof(TAggregateEvent);

    public TIdentity AggregateIdentity { get; }
    public TAggregateEvent AggregateEvent { get; }
    public long AggregateSequenceNumber { get; }
    public Metadata Metadata { get; }
    public DateTimeOffset Timestamp { get; }

    public IIdentity GetIdentity() => AggregateIdentity;

    public IAggregateEvent GetAggregateEvent() => AggregateEvent;

    public override string ToString()
        => $"{AggregateType.PrettyPrint()} v{AggregateSequenceNumber}/{EventType.PrettyPrint()}:{AggregateIdentity}";
}