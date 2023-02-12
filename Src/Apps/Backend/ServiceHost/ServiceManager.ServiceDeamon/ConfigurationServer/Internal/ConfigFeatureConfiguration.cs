using System;
using ServiceHost.ClientApp.Shared.ConfigurationServer.Data;
using ServiceHost.ClientApp.Shared.ConfigurationServer.Events;
using ServiceManager.ServiceDeamon.ConfigurationServer.Data;
using SharpRepository.Repository;

namespace ServiceManager.ServiceDeamon.ConfigurationServer.Internal
{
    public sealed record ConfigFeatureConfiguration(
        ServerConfigugration Configugration, IRepository<GlobalConfigEntity, string> GlobalRepository, IRepository<SeedUrlEntity, string> Seeds,
        IRepository<SpecificConfigEntity, string> Apps, IObservable<IConfigEvent> EventPublisher, Action<IConfigEvent> EventSender);
}