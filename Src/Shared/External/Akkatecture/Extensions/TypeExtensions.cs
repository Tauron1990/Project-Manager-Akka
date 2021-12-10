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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Akkatecture.Aggregates;
using Akkatecture.Aggregates.Snapshot;
using Akkatecture.Core;
using Akkatecture.Events;
using Akkatecture.Jobs;
using Akkatecture.Sagas;
using Akkatecture.Sagas.SagaTimeouts;
using Akkatecture.Subscribers;

namespace Akkatecture.Extensions;

public static class TypeExtensions
{
    private static readonly ConcurrentDictionary<Type, string> PrettyPrintCache = new();

    private static readonly ConcurrentDictionary<Type, AggregateName> AggregateNames = new();


    private static readonly ConcurrentDictionary<Type, AggregateName> SagaNames = new();

    private static readonly ConcurrentDictionary<Type, JobName> JobNames = new();

    private static readonly ConcurrentDictionary<Type, AggregateName> AggregateNameCache = new();

    private static readonly ConcurrentDictionary<Type, Type> AggregateEventTypeCache = new();

    public static string PrettyPrint(this Type type)
    {
        return PrettyPrintCache.GetOrAdd(
            type,
            newType =>
            {
                try
                {
                    return PrettyPrintRecursive(newType, 0);
                }
                catch (Exception)
                {
                    #pragma warning disable ERP022
                    return newType.Name;
                    #pragma warning restore ERP022
                }
            });
    }

    private static string PrettyPrintRecursive(Type type, int depth)
    {
        if (depth > 3) return type.Name;

        var nameParts = type.Name.Split('`');

        if (nameParts.Length == 1) return nameParts[0];

        var genericArguments = type.GetTypeInfo().GetGenericArguments();

        return !type.IsConstructedGenericType
            ? $"{nameParts[0]}<{new string(',', genericArguments.Length - 1)}>"
            : $"{nameParts[0]}<{string.Join(",", genericArguments.Select(selectType => PrettyPrintRecursive(selectType, depth + 1)))}>";
    }

    public static AggregateName GetAggregateName(
        this Type aggregateType)
    {
        return AggregateNames.GetOrAdd(
            aggregateType,
            type =>
            {
                if (!typeof(IAggregateRoot).GetTypeInfo().IsAssignableFrom(type))
                    throw new ArgumentException($"Type '{type.PrettyPrint()}' is not an aggregate root");

                return new AggregateName(
                    type.GetTypeInfo().GetCustomAttributes<AggregateNameAttribute>().SingleOrDefault()?.Name ??
                    type.Name);
            });
    }

    public static AggregateName GetSagaName(
        this Type sagaType)
    {
        return SagaNames.GetOrAdd(
            sagaType,
            type =>
            {
                if (!typeof(IAggregateRoot).GetTypeInfo().IsAssignableFrom(type))
                    throw new ArgumentException($"Type '{type.PrettyPrint()}' is not a saga.");

                return new AggregateName(
                    type.GetTypeInfo().GetCustomAttributes<SagaNameAttribute>().SingleOrDefault()?.Name ??
                    type.Name);
            });
    }

    public static JobName GetJobName(
        this Type jobType)
    {
        return JobNames.GetOrAdd(
            jobType,
            type =>
            {
                if (!typeof(IJob).GetTypeInfo().IsAssignableFrom(type))
                    throw new ArgumentException($"Type '{type.PrettyPrint()}' is not a job");

                return new JobName(
                    type.GetTypeInfo().GetCustomAttributes<JobNameAttribute>().SingleOrDefault()?.Name ??
                    type.Name);
            });
    }

    internal static IReadOnlyDictionary<Type, Action<T, IAggregateEvent>> GetAggregateEventApplyMethods<TAggregate,
        TIdentity, T>(this Type type)
        where TAggregate : IAggregateRoot<TIdentity>
        where TIdentity : IIdentity
    {
        var aggregateEventType = typeof(IAggregateEvent<TAggregate, TIdentity>);

        return type
           .GetTypeInfo()
           .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
           .Where(
                mi =>
                {
                    if (mi.Name != "Apply") return false;

                    var parameters = mi.GetParameters();

                    return
                        parameters.Length == 1 &&
                        aggregateEventType.GetTypeInfo().IsAssignableFrom(parameters[0].ParameterType);
                })
           .ToImmutableDictionary(
                mi => mi.GetParameters()[0].ParameterType,
                mi => ReflectionHelper.CompileMethodInvocation<Action<T, IAggregateEvent>>(
                    type,
                    "Apply",
                    mi.GetParameters()[0].ParameterType));
    }

    internal static IReadOnlyDictionary<Type, Action<T?, IAggregateSnapshot?>> GetAggregateSnapshotHydrateMethods<
        TAggregate, TIdentity, T>(this Type type)
        where TAggregate : IAggregateRoot<TIdentity>
        where TIdentity : IIdentity
        where T : class
    {
        var aggregateSnapshot = typeof(IAggregateSnapshot<TAggregate, TIdentity>);

        return type
           .GetTypeInfo()
           .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
           .Where(
                mi =>
                {
                    if (mi.Name != "Hydrate") return false;

                    var parameters = mi.GetParameters();

                    return
                        parameters.Length == 1 &&
                        aggregateSnapshot.GetTypeInfo().IsAssignableFrom(parameters[0].ParameterType);
                })
           .ToImmutableDictionary(
                mi => mi.GetParameters()[0].ParameterType,
                mi => ReflectionHelper.CompileMethodInvocation<Action<T?, IAggregateSnapshot?>>(
                    type,
                    "Hydrate",
                    mi.GetParameters()[0].ParameterType));
    }

    internal static IReadOnlyDictionary<Type, Action<TAggregateState?, IAggregateEvent?>>
        GetAggregateStateEventApplyMethods<TAggregate, TIdentity, TAggregateState>(this Type type)
        where TAggregate : IAggregateRoot<TIdentity>
        where TIdentity : IIdentity
        where TAggregateState : class, IEventApplier<TAggregate, TIdentity>
    {
        var aggregateEventType = typeof(IAggregateEvent<TAggregate, TIdentity>);

        return type
           .GetTypeInfo()
           .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
           .Where(
                mi =>
                {
                    if (mi.Name != "Apply") return false;

                    var parameters = mi.GetParameters();

                    return
                        parameters.Length == 1 &&
                        aggregateEventType.GetTypeInfo().IsAssignableFrom(parameters[0].ParameterType);
                })
           .ToImmutableDictionary(
                mi => mi.GetParameters()[0].ParameterType,
                mi => ReflectionHelper.CompileMethodInvocation<Action<TAggregateState?, IAggregateEvent?>>(
                    type,
                    "Apply",
                    mi.GetParameters()[0].ParameterType));
    }

    internal static IReadOnlyList<Type> GetAsyncDomainEventSubscriberSubscriptionTypes(this Type type)
    {
        var interfaces = type
           .GetTypeInfo()
           .GetInterfaces()
           .Select(selectType => selectType.GetTypeInfo());

        var domainEventTypes = interfaces
           .Where(typeInfo => typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof(ISubscribeToAsync<,,>))
           .Select(
                typeInfo => typeof(IDomainEvent<,,>).MakeGenericType(
                    typeInfo.GetGenericArguments()[0],
                    typeInfo.GetGenericArguments()[1],
                    typeInfo.GetGenericArguments()[2]))
           .ToImmutableList();


        return domainEventTypes;
    }

    internal static IReadOnlyList<Type> GetAggregateExecuteTypes(this Type type)
    {
        var interfaces = type
           .GetTypeInfo()
           .GetInterfaces()
           .Select(selectType => selectType.GetTypeInfo());

        var domainEventTypes = interfaces
           .Where(typeInfo => typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof(IExecute<>))
           .Select(typeInfo => typeInfo.GetGenericArguments()[0])
           .ToImmutableList();


        return domainEventTypes;
    }

    internal static IReadOnlyList<Type> GetDomainEventSubscriberSubscriptionTypes(this Type type)
    {
        var interfaces = type
           .GetTypeInfo()
           .GetInterfaces()
           .Select(selectType => selectType.GetTypeInfo());

        var domainEventTypes = interfaces
           .Where(typeInfo => typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof(ISubscribeTo<,,>))
           .Select(
                typeInfo => typeof(IDomainEvent<,,>).MakeGenericType(
                    typeInfo.GetGenericArguments()[0],
                    typeInfo.GetGenericArguments()[1],
                    typeInfo.GetGenericArguments()[2]))
           .ToImmutableList();


        return domainEventTypes;
    }

    internal static AggregateName GetCommittedEventAggregateRootName(this Type type)
    {
        return AggregateNameCache.GetOrAdd(
            type,
            creatorType =>
            {
                var interfaces = creatorType
                   .GetTypeInfo()
                   .GetInterfaces()
                   .Select(selectorType => selectorType.GetTypeInfo())
                   .ToList();

                var aggregateType = interfaces
                   .Where(typeInfo => typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof(ICommittedEvent<,>))
                   .Select(typeInfo => typeInfo.GetGenericArguments()[0]).SingleOrDefault();


                if (aggregateType != null)
                    return aggregateType.GetAggregateName();

                #pragma warning disable CA2208
                throw new ArgumentException(nameof(type));
                #pragma warning restore CA2208
            });
    }

    internal static Type GetCommittedEventAggregateEventType(this Type type)
    {
        return AggregateEventTypeCache.GetOrAdd(
            type,
            newType =>
            {
                var interfaces = newType
                   .GetTypeInfo()
                   .GetInterfaces()
                   .Select(selectorType => selectorType.GetTypeInfo());

                var aggregateEvent = interfaces
                   .Where(typeInfo => typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof(ICommittedEvent<,,>))
                   .Select(typeInfo => typeInfo.GetGenericArguments()[2]).SingleOrDefault();


                if (aggregateEvent != null)
                    return aggregateEvent;

                #pragma warning disable CA2208
                throw new ArgumentException(nameof(type));
                #pragma warning restore CA2208
            });
    }

    internal static IReadOnlyList<Type> GetJobRunTypes(this Type type)
    {
        var interfaces = type
           .GetTypeInfo()
           .GetInterfaces()
           .Select(selectorType => selectorType.GetTypeInfo());

        var jobRunTypes = interfaces
           .Where(typeInfo => typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof(IRun<>))
           .Select(typeInfo => typeInfo.GetGenericArguments()[0])
           .ToImmutableList();


        return jobRunTypes;
    }

    internal static IReadOnlyList<Type> GetAsyncSagaEventSubscriptionTypes(this Type type)
    {
        var interfaces = type
           .GetTypeInfo()
           .GetInterfaces()
           .Select(selectorType => selectorType.GetTypeInfo())
           .ToImmutableList();

        var handleEventTypes = interfaces
           .Where(typeInfo => typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof(ISagaHandlesAsync<,,>))
           .Select(
                typeInfo => typeof(IDomainEvent<,,>).MakeGenericType(
                    typeInfo.GetGenericArguments()[0],
                    typeInfo.GetGenericArguments()[1],
                    typeInfo.GetGenericArguments()[2]));

        var startedByEventTypes = interfaces
           .Where(typeInfo => typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof(ISagaIsStartedByAsync<,,>))
           .Select(
                typeInfo => typeof(IDomainEvent<,,>).MakeGenericType(
                    typeInfo.GetGenericArguments()[0],
                    typeInfo.GetGenericArguments()[1],
                    typeInfo.GetGenericArguments()[2]))
           .Concat(handleEventTypes)
           .ToImmutableList();

        return startedByEventTypes;
    }

    internal static IReadOnlyList<Type> GetSagaEventSubscriptionTypes(this Type type)
    {
        var interfaces = type
           .GetTypeInfo()
           .GetInterfaces()
           .Select(selectorType => selectorType.GetTypeInfo())
           .ToImmutableList();

        var handleEventTypes = interfaces
           .Where(typeInfo => typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof(ISagaHandles<,,>))
           .Select(
                typeInfo => typeof(IDomainEvent<,,>).MakeGenericType(
                    typeInfo.GetGenericArguments()[0],
                    typeInfo.GetGenericArguments()[1],
                    typeInfo.GetGenericArguments()[2]));

        var startedByEventTypes = interfaces
           .Where(typeInfo => typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof(ISagaIsStartedBy<,,>))
           .Select(
                typeInfo => typeof(IDomainEvent<,,>).MakeGenericType(
                    typeInfo.GetGenericArguments()[0],
                    typeInfo.GetGenericArguments()[1],
                    typeInfo.GetGenericArguments()[2]))
           .Concat(handleEventTypes)
           .ToImmutableList();

        return startedByEventTypes;
    }

    internal static IReadOnlyList<Type> GetAsyncSagaTimeoutSubscriptionTypes(this Type type)
    {
        var interfaces = type
           .GetTypeInfo()
           .GetInterfaces()
           .Select(selectorType => selectorType.GetTypeInfo());

        var sagaTimeoutSubscriptionTypes = interfaces
           .Where(typeInfo => typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof(ISagaHandlesTimeoutAsync<>))
           .Select(typeInfo => typeInfo.GetGenericArguments()[0])
           .ToImmutableList();

        return sagaTimeoutSubscriptionTypes;
    }

    internal static IReadOnlyList<Type> GetSagaTimeoutSubscriptionTypes(this Type type)
    {
        var interfaces = type
           .GetTypeInfo()
           .GetInterfaces()
           .Select(selectType => selectType.GetTypeInfo());

        var sagaTimeoutSubscriptionTypes = interfaces
           .Where(typeInfo => typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof(ISagaHandlesTimeout<>))
           .Select(typeInfo => typeInfo.GetGenericArguments()[0])
           .ToImmutableList();

        return sagaTimeoutSubscriptionTypes;
    }

    internal static IReadOnlyDictionary<Type, Func<T, IAggregateEvent, IAggregateEvent>>
        GetAggregateEventUpcastMethods<TAggregate, TIdentity, T>(this Type type)
        where TAggregate : IAggregateRoot<TIdentity>
        where TIdentity : IIdentity
    {
        var aggregateEventType = typeof(IAggregateEvent<TAggregate, TIdentity>);

        return type
           .GetTypeInfo()
           .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
           .Where(
                mi =>
                {
                    if (mi.Name != "Upcast")
                        return false;

                    var parameters = mi.GetParameters();

                    return
                        parameters.Length == 1 &&
                        aggregateEventType.GetTypeInfo().IsAssignableFrom(parameters[0].ParameterType);
                })
           .ToImmutableDictionary(
                //problem might be here
                mi => mi.GetParameters()[0].ParameterType,
                mi => ReflectionHelper.CompileMethodInvocation<Func<T, IAggregateEvent, IAggregateEvent>>(
                    type,
                    "Upcast",
                    mi.GetParameters()[0].ParameterType));
    }

    internal static IReadOnlyList<Type> GetAggregateEventUpcastTypes(this Type type)
    {
        var interfaces = type
           .GetTypeInfo()
           .GetInterfaces()
           .Select(selectType => selectType.GetTypeInfo());

        var upcastableEventTypes = interfaces
           .Where(typeInfo => typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof(IUpcast<,>))
           .Select(typeInfo => typeInfo.GetGenericArguments()[0])
           .ToImmutableList();

        return upcastableEventTypes;
    }

    internal static Type GetBaseType(this Type type, string name)
    {
        var currentType = type;

        while (currentType != null)
        {
            if (currentType.Name.Contains(name)) return currentType;

            currentType = currentType.BaseType;
        }

        return type;
    }
}