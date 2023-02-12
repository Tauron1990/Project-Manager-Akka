using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Akka.Actor;
using ServiceHost.ClientApp.Shared.ConfigurationServer;
using ServiceHost.ClientApp.Shared.ConfigurationServer.Data;
using ServiceHost.ClientApp.Shared.ConfigurationServer.Events;
using ServiceManager.ServiceDeamon.ConfigurationServer.Data;
using ServiceManager.ServiceDeamon.ConfigurationServer.Internal;
using ServiceManager.ServiceDeamon.Management;
using SharpRepository.Repository;
using SharpRepository.Repository.Configuration;
using Tauron;
using Tauron.Application.AkkaNode.Services.Core;
using Tauron.Features;

namespace ServiceManager.ServiceDeamon.ConfigurationServer
{
    public sealed class ConfigurationManagerActor : ActorFeatureBase<ConfigurationManagerActor.State>
    {
        private const string ServerConfigurationId = "86FCF2E7-10D0-4103-82F4-D0F0E08995EF";

        public static Props New(ISharpRepositoryConfiguration repository)
        {
            var serverConfiguration = repository.GetInstance<ServerConfigurationEntity, string>(ServiceManagerDeamon.RepositoryKey);
            var publisher = new Subject<IConfigEvent>();

            ConfigFeatureConfiguration CreateState()
                => new(
                    serverConfiguration.Get(ServerConfigurationId)?.Configugration ?? new ServerConfigugration(MonitorChanges: true, RestartServices: false, string.Empty),
                    repository.GetInstance<GlobalConfigEntity, string>(ServiceManagerDeamon.RepositoryKey),
                    repository.GetInstance<SeedUrlEntity, string>(ServiceManagerDeamon.RepositoryKey),
                    repository.GetInstance<SpecificConfigEntity, string>(ServiceManagerDeamon.RepositoryKey),
                    publisher.ObserveOn(Scheduler.Default),
                    publisher.AsObserver().OnNext);

            return Feature.Props(Feature.Create(() => new ConfigurationManagerActor(), _ => new State(CreateState, serverConfiguration, publisher)), TellAliveFeature.New());
        }

        protected override void ConfigImpl()
        {
            var querys = Context.ActorOf("Querys", Feature.Create(() => new ConfigQueryFeature(), _ => CurrentState.Factory()));
            var commands = Context.ActorOf("Commands", Feature.Create(() => new ConfigCommandFeature(), _ => CurrentState.Factory()));

            Context.ActorOf("HostUpdateManger", HostupdateManagerFeature.New(CurrentState.EventPublisher.ObserveOn(Scheduler.Default), CurrentState.Factory()));

            Receive<IConfigQuery>(
                obs => obs.Select(o => o.Event)
                   .ToActor(querys));
            Receive<IConfigCommand>(
                obs => obs.Select(c => c.Event)
                   .ToActor(commands));
            Receive<ServerConfigugration>(
                obs => obs.ToUnit(
                    sp =>
                    {
                        var (evt, (_, serverConfiguration, eventPublisher)) = sp;
                        if (!serverConfiguration.Exists(ServerConfigurationId))
                            serverConfiguration.Add(new ServerConfigurationEntity(ServerConfigurationId, evt));
                        else
                            serverConfiguration.Update(new ServerConfigurationEntity(ServerConfigurationId, evt));
                        eventPublisher.OnNext(new ServerConfigurationEvent(evt));
                    }));
        }

        public sealed record State(Func<ConfigFeatureConfiguration> Factory, IRepository<ServerConfigurationEntity, string> ServerConfiguration, ISubject<IConfigEvent> EventPublisher);
    }
}