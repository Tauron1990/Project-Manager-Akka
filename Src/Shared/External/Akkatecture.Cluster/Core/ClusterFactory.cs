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
using System.Linq.Expressions;
using Akka.Actor;
using Akka.Cluster.Sharding;
using Akka.Persistence;
using Akkatecture.Aggregates;
using Akkatecture.Cluster.Dispatchers;
using Akkatecture.Core;
using Akkatecture.Sagas;
using Akkatecture.Sagas.AggregateSaga;
using JetBrains.Annotations;

namespace Akkatecture.Cluster.Core;

[PublicAPI]
public static class ClusterFactory<TAggregateManager, TAggregate, TIdentity>
    where TAggregateManager : ReceiveActor, IAggregateManager<TAggregate, TIdentity>
    where TAggregate : ReceivePersistentActor, IAggregateRoot<TIdentity>
    where TIdentity : IIdentity
{
    #pragma warning disable AV1551
    #pragma warning disable MA0018
    public static IActorRef StartClusteredAggregate(
        ActorSystem actorSystem,
        int numberOfShards = 12)
    {
        ClusterSharding clusterSharding = ClusterSharding.Get(actorSystem);
        ClusterShardingSettings clusterShardingSettings = clusterSharding.Settings;

        var aggregateManagerProps = Props.Create<TAggregateManager>();

        IActorRef shardRef = clusterSharding.Start(
            typeof(TAggregateManager).Name,
            Props.Create(() => new ClusterParentProxy(aggregateManagerProps, true)),
            clusterShardingSettings,
            new MessageExtractor<TAggregate, TIdentity>(numberOfShards)
        );

        return shardRef;
    }

    public static IActorRef StartClusteredAggregate(
        ActorSystem actorSystem,
        Expression<Func<TAggregateManager>> aggregateManagerFactory,
        int numberOfShards = 12)
    {
        ClusterSharding clusterSharding = ClusterSharding.Get(actorSystem);
        ClusterShardingSettings clusterShardingSettings = clusterSharding.Settings;

        var aggregateManagerProps = Props.Create(aggregateManagerFactory);

        IActorRef shardRef = clusterSharding.Start(
            typeof(TAggregateManager).Name,
            Props.Create(() => new ClusterParentProxy(aggregateManagerProps, false)),
            clusterShardingSettings,
            new MessageExtractor<TAggregate, TIdentity>(numberOfShards)
        );

        return shardRef;
    }

    public static IActorRef StartAggregateClusterProxy(
        ActorSystem actorSystem,
        string clusterRoleName,
        int numberOfShards = 12)
    {
        ClusterSharding clusterSharding = ClusterSharding.Get(actorSystem);

        IActorRef shardRef = clusterSharding.StartProxy(
            typeof(TAggregateManager).Name,
            clusterRoleName,
            new MessageExtractor<TAggregate, TIdentity>(numberOfShards)
        );

        return shardRef;
    }
}

public static class ClusterFactory<TAggregateSagaManager, TAggregateSaga, TIdentity, TSagaLocator>
    where TAggregateSagaManager : ReceiveActor, IAggregateSagaManager<TAggregateSaga, TIdentity, TSagaLocator>
    where TAggregateSaga : ReceivePersistentActor, IAggregateSaga<TIdentity>
    where TIdentity : SagaId<TIdentity>
    where TSagaLocator : class, ISagaLocator<TIdentity>, new()
{
    public static IActorRef StartClusteredAggregateSaga(
        ActorSystem actorSystem,
        Expression<Func<TAggregateSaga>> sagaFactory,
        string clusterRoleName,
        int numberOfShards = 12)
    {
        if(sagaFactory is null) throw new ArgumentNullException(nameof(sagaFactory));

        ClusterSharding clusterSharding = ClusterSharding.Get(actorSystem);
        ClusterShardingSettings clusterShardingSettings = clusterSharding.Settings;

        // ReSharper disable once PossiblyMistakenUseOfParamsMethod
        var aggregateSagaManagerProps = Props.Create<TAggregateSagaManager>(sagaFactory);

        IActorRef shardRef = clusterSharding.Start(
            typeof(TAggregateSagaManager).Name,
            Props.Create(() => new ClusterParentProxy(aggregateSagaManagerProps, true)),
            clusterShardingSettings,
            new MessageExtractor<TAggregateSagaManager, TAggregateSaga, TIdentity, TSagaLocator>(numberOfShards)
        );


        actorSystem.ActorOf(
            Props.Create(
                () =>
                    new ShardedAggregateSagaDispatcher<TAggregateSagaManager, TAggregateSaga, TIdentity, TSagaLocator>(
                        clusterRoleName,
                        numberOfShards)),
            $"{typeof(TAggregateSaga).Name}Dispatcher");

        return shardRef;
    }

    public static IActorRef StartAggregateSagaClusterProxy(
        ActorSystem actorSystem,
        string clusterRoleName,
        int numberOfShards = 12)
    {
        ClusterSharding clusterSharding = ClusterSharding.Get(actorSystem);

        IActorRef shardRef = clusterSharding.StartProxy(
            typeof(TAggregateSagaManager).Name,
            clusterRoleName,
            new MessageExtractor<TAggregateSagaManager, TAggregateSaga, TIdentity, TSagaLocator>(numberOfShards)
        );

        return shardRef;
    }
}
#pragma warning restore AV1551
#pragma warning restore MA0018