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
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Akkatecture.Aggregates;
using Akkatecture.Subscribers;

namespace Akkatecture.Cluster.Extentions;

public static class TypeExtensions
{
    internal static IReadOnlyList<Type> GetDomainEventSubscriptionTypes(this Type type)
    {
        var interfaces = type
           .GetTypeInfo()
           .GetInterfaces()
           .Select(selectorType => selectorType.GetTypeInfo())
           .ToImmutableList();

        var asyncDomainEventTypes = interfaces
           .Where(info => info.IsGenericType && info.GetGenericTypeDefinition() == typeof(ISubscribeToAsync<,,>))
           .Select(
                info => typeof(IDomainEvent<,,>).MakeGenericType(
                    info.GetGenericArguments()[0],
                    info.GetGenericArguments()[1],
                    info.GetGenericArguments()[2]));

        var domainEventTypes = interfaces
           .Where(info => info.IsGenericType && info.GetGenericTypeDefinition() == typeof(ISubscribeTo<,,>))
           .Select(
                info => typeof(IDomainEvent<,,>).MakeGenericType(
                    info.GetGenericArguments()[0],
                    info.GetGenericArguments()[1],
                    info.GetGenericArguments()[2]))
           .Concat(asyncDomainEventTypes)
           .ToImmutableList();


        return domainEventTypes;
    }
}