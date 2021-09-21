using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Stl.Fusion.AkkaBridge.Connector;

namespace Stl.Fusion.AkkaBridge.Internal
{
    public sealed class ServicePublisherHost : BackgroundService
    {
        private readonly ILogger<ServicePublisherHost> _log;
        private readonly IServiceRegistryActor _registryActor;
        private readonly IServiceProvider _serviceProvider;
        private readonly IEnumerable<PublishService> _services;
        private readonly ActorSystem _system;

        public ServicePublisherHost(IEnumerable<PublishService> services, IServiceRegistryActor registryActor, IServiceProvider serviceProvider, ActorSystem system, ILogger<ServicePublisherHost> log)
        {
            _services = services;
            _registryActor = registryActor;
            _serviceProvider = serviceProvider;
            _system = system;
            _log = log;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            foreach (var service in _services)
            {
                var response = await _registryActor.RegisterService(
                    new RegisterService(service.ServiceType, ServiceHostActor.CreateHost(_system, service.Resolver(), service.ServiceType, _serviceProvider)),
                    TimeSpan.FromSeconds(10));
                if (response.Error != null)
                    _log.LogError(response.Error, "Error on Register Service {Name}", service.ServiceType);
            }
        }
    }
}