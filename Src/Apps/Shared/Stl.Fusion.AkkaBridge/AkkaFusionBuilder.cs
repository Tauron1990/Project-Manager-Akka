using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Stl.CommandR;
using Stl.Fusion.AkkaBridge.Connector;
using Stl.Fusion.AkkaBridge.Internal;
using Stl.Fusion.Bridge.Interception;
using Stl.Fusion.Interception;
using Tauron;

namespace Stl.Fusion.AkkaBridge
{
    public readonly struct AkkaFusionBuilder
    {
        public FusionBuilder FusionBuilder { get; }

        public AkkaFusionBuilder(FusionBuilder fusionFusionBuilder)
            => FusionBuilder = fusionFusionBuilder;

        public AkkaFusionBuilder AddReplicaService<TService>(bool isCommandService = true)
        {
            var serviceType = typeof(TService);

            if (!serviceType.IsInterface || !serviceType.IsPublic)
                throw new InvalidOperationException("Public and Interface Type Requierd");

            if (FusionBuilder.Services.Any(d => d.ServiceType == serviceType))
                return this;

            var serviceAccessorType = typeof(ServiceAccessor<>).MakeGenericType(serviceType);

            object ServiceAccessorFactory(IServiceProvider c)
            {
                var replicaMethodInterceptor = c.GetRequiredService<ReplicaMethodInterceptor>();
                replicaMethodInterceptor.ValidateType(serviceType);
                var commandMethodInterceptor = c.GetRequiredService<ComputeMethodInterceptor>();
                commandMethodInterceptor.ValidateType(serviceType);

                var client = c.GetRequiredService<AkkaProxyGenerator>().GenerateAkkaProxy(serviceType);

                return FastReflection.Shared.GetCreator(serviceAccessorType, new[] { serviceType })?.Invoke(new[] { client })
                    ?? throw new InvalidCastException("Client Accessor was not created"); //serviceAccessorType!.CreateInstance(client);
            }

            object ServiceFactory(IServiceProvider c)
            {
                var clientAccessor = (IServiceAccessor)c.GetRequiredService(serviceAccessorType);
                var client = clientAccessor.Service;

                
                // 4. Create Replica Client
                var replicaProxyGenerator = c.GetRequiredService<IReplicaServiceProxyGenerator>();
                var replicaProxyType = replicaProxyGenerator.GetProxyType(serviceType, isCommandService);
                var replicaInterceptors = c.GetRequiredService<ReplicaServiceInterceptor[]>();
                client = replicaProxyType.CreateInstance(replicaInterceptors, client);

                return client;
            }

            FusionBuilder.Services.AddSingleton(serviceAccessorType, ServiceAccessorFactory);
            FusionBuilder.Services.AddSingleton(serviceType, ServiceFactory);
            if (isCommandService)
                FusionBuilder.Services.AddCommander().AddCommandService(serviceType);

            return this;
        }
    }
}