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

using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Reflection;
using Akkatecture.Aggregates;
using Akkatecture.Aggregates.Snapshot;
using Akkatecture.Core;
using Akkatecture.Events;
using Akkatecture.Jobs;
using Akkatecture.Sagas;
using Akkatecture.Sagas.SagaTimeouts;
using Akkatecture.Subscribers;
using JetBrains.Annotations;

namespace Akkatecture.Extensions;

[PublicAPI]
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
        if(depth > 3) return type.Name;

        string[] nameParts = type.Name.Split('`');

        if(nameParts.Length == 1) return nameParts[0];

        var genericArguments = type.GetTypeInfo().GetGenericArguments();

        return !type.IsConstructedGenericType
            ? $"{nameParts[0]}<{new string(',', genericArguments.Length - 1)}>"
            : $"{nameParts[0]}<{string.Join(",", genericArguments.Select(selectType => PrettyPrintRecursive(selectType, depth + 1)))}>";
    }

    public static AggregateName GetAggregateName(this Type aggregateType)
    {
        return AggregateNames.GetOrAdd(
            aggregateType,
            type =>
            {
                if(!typeof(IAggregateRoot).GetTypeInfo().IsAssignableFrom(type))
                    throw new ArgumentException($"Type '{type.PrettyPrint()}' is not an aggregate root", nameof(aggregateType));

                return new AggregateName(
                    type.GetTypeInfo().GetCustomAttributes<AggregateNameAttribute>().SingleOrDefault()?.Name ??
                    type.Name);
            });
    }

    public static AggregateName GetSagaName(this Type sagaType)
    {
        return SagaNames.GetOrAdd(
            sagaType,
            type =>
            {
                if(!typeof(IAggregateRoot).GetTypeInfo().IsAssignableFrom(type))
                    throw new ArgumentException($"Type '{type.PrettyPrint()}' is not a saga.", nameof(sagaType));

                return new AggregateName(
                    type.GetTypeInfo().GetCustomAttributes<SagaNameAttribute>().SingleOrDefault()?.Name ??
                    type.Name);
            });
    }

    public static JobName GetJobName(this Type jobType)
    {
        return JobNames.GetOrAdd(
            jobType,
            type =>
            {
                if(!typeof(IJob).GetTypeInfo().IsAssignableFrom(type))
                    throw new ArgumentException($"Type '{type.PrettyPrint()}' is not a job", nameof(jobType));

                return new JobName(
                    type.GetTypeInfo().GetCustomAttributes<JobNameAttribute>().SingleOrDefault()?.Name ??
                    type.Name);
            });
    }

    public static IReadOnlyDictionary<Type, Action<T, IAggregateEvent>> GetAggregateEventApplyMethods<TAggregate,
        TIdentity, T>(this Type type)
        where TAggregate : IAggregateRoot<TIdentity>
        where TIdentity : IIdentity
    {
        Type aggregateEventType = typeof(IAggregateEvent<TAggregate, TIdentity>);

        return type
           .GetTypeInfo()
           .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
           .Where(
                mi =>
                {
                    if(!string.Equals(mi.Name, "Apply", StringComparison.Ordinal)) return false;

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

    public static IReadOnlyDictionary<Type, Action<T?, IAggregateSnapshot?>> GetAggregateSnapshotHydrateMethods<
        TAggregate, TIdentity, T>(this Type type)
        where TAggregate : IAggregateRoot<TIdentity>
        where TIdentity : IIdentity
        where T : class
    {
        Type aggregateSnapshot = typeof(IAggregateSnapshot<TAggregate, TIdentity>);

        return type
           .GetTypeInfo()
           .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
           .Where(
                mi =>
                {
                    if(!string.Equals(mi.Name, "Hydrate", StringComparison.Ordinal)) return false;

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

    public static IReadOnlyDictionary<Type, Action<TAggregateState?, IAggregateEvent?>>
        GetAggregateStateEventApplyMethods<TAggregate, TIdentity, TAggregateState>(this Type type)
        where TAggregate : IAggregateRoot<TIdentity>
        where TIdentity : IIdentity
        where TAggregateState : class, IEventApplier<TAggregate, TIdentity>
    {
        Type aggregateEventType = typeof(IAggregateEvent<TAggregate, TIdentity>);

        return type
           .GetTypeInfo()
           .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
           .Where(
                mi =>
                {
                    if(!string.Equals(mi.Name, "Apply", StringComparison.Ordinal)) return false;

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

    public static IReadOnlyList<Type> GetAsyncDomainEventSubscriberSubscriptionTypes(this Type type)
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

    public static IReadOnlyList<Type> GetAggregateExecuteTypes(this Type type)
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

    public static IReadOnlyList<Type> GetDomainEventSubscriberSubscriptionTypes(this Type type)
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

    public static AggregateName GetCommittedEventAggregateRootName(this Type type)
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

                Type? aggregateType = interfaces
                   .Where(typeInfo => typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof(ICommittedEvent<,>))
                   .Select(typeInfo => typeInfo.GetGenericArguments()[0])
                   .SingleOrDefault();


                if(aggregateType != null)
                    return aggregateType.GetAggregateName();

                #pragma warning disable CA2208
                throw new ArgumentException("The aggregate Type was not Found", nameof(type));
                #pragma warning restore CA2208
            });
    }

    public static Type GetCommittedEventAggregateEventType(this Type type)
    {
        return AggregateEventTypeCache.GetOrAdd(
            type,
            newType =>
            {
                var interfaces = newType
                   .GetTypeInfo()
                   .GetInterfaces()
                   .Select(selectorType => selectorType.GetTypeInfo());

                Type? aggregateEvent = interfaces
                   .Where(typeInfo => typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof(ICommittedEvent<,,>))
                   .Select(typeInfo => typeInfo.GetGenericArguments()[2]).SingleOrDefault();


                if(aggregateEvent != null)
                    return aggregateEvent;

                #pragma warning disable CA2208
                throw new ArgumentException("The Aggregate Event eas not Found", nameof(type));
                #pragma warning restore CA2208
            });
    }

    public static IReadOnlyList<Type> GetJobRunTypes(this Type type)
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

    public static IReadOnlyList<Type> GetAsyncSagaEventSubscriptionTypes(this Type type)
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

    public static IReadOnlyList<Type> GetSagaEventSubscriptionTypes(this Type type)
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

    public static IReadOnlyList<Type> GetAsyncSagaTimeoutSubscriptionTypes(this Type type)
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

    public static IReadOnlyList<Type> GetSagaTimeoutSubscriptionTypes(this Type type)
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

    public static IReadOnlyDictionary<Type, Func<T, IAggregateEvent, IAggregateEvent>>
        GetAggregateEventUpcastMethods<TAggregate, TIdentity, T>(this Type type)
        where TAggregate : IAggregateRoot<TIdentity>
        where TIdentity : IIdentity
    {
        Type aggregateEventType = typeof(IAggregateEvent<TAggregate, TIdentity>);

        return type
           .GetTypeInfo()
           .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
           .Where(
                mi =>
                {
                    if(!string.Equals(mi.Name, "Upcast", StringComparison.Ordinal))
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

    public static IReadOnlyList<Type> GetAggregateEventUpcastTypes(this Type type)
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

    public static Type GetBaseType(this Type type, string name)
    {
        Type? currentType = type;

        while (currentType != null)
        {
            if(currentType.Name.Contains(name, StringComparison.Ordinal)) return currentType;

            currentType = currentType.BaseType;
        }

        return type;
    }
}