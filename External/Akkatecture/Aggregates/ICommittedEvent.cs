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
using JetBrains.Annotations;

namespace Akkatecture.Aggregates
{
    [PublicAPI]
    public interface ICommittedEvent
    {
        long AggregateSequenceNumber { get; }
        Metadata Metadata { get; }
        DateTimeOffset Timestamp { get; }

        IIdentity GetIdentity();
        IAggregateEvent GetAggregateEvent();
    }

    [PublicAPI]
    public interface ICommittedEvent<TAggregate, out TIdentity> : ICommittedEvent
        where TAggregate : IAggregateRoot<TIdentity>
        where TIdentity : IIdentity
    {
        TIdentity AggregateIdentity { get; }
    }

    [PublicAPI]
    public interface ICommittedEvent<TAggregate, out TIdentity, out TAggregateEvent> : ICommittedEvent<TAggregate, TIdentity>
        where TAggregate : IAggregateRoot<TIdentity>
        where TIdentity : IIdentity
        where TAggregateEvent : class, IAggregateEvent<TAggregate, TIdentity>
    {
        TAggregateEvent AggregateEvent { get; }
    }
}