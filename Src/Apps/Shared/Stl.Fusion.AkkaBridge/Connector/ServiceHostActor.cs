using System;
using System.Reflection;
using System.Threading.Tasks;
using Akka.Actor;
using Microsoft.Extensions.DependencyInjection;
using Stl.Fusion.Bridge;
using Tauron;

namespace Stl.Fusion.AkkaBridge.Connector
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public sealed class ServiceHostActor : ReceiveActor
    {
        private IMethodInvoker _methodInvoker;

        private IPublisher _publisher;

        public ServiceHostActor()
        {
            Receive<RequestPublication>(p => RunPublication(p).PipeTo(Sender));
            Receive<TryMethodCall>(m => _methodInvoker!.TryInvoke(m).PipeTo(Sender));
            Receive<Init>(
                i =>
                {
                    var (target, mapping, serviceProvider) = i;
                    _publisher = serviceProvider.GetRequiredService<IPublisher>();
                    var proxyGenerator = serviceProvider.GetRequiredService<IComputeServiceProxyGenerator>();
                    var proxyType = proxyGenerator.GetProxyType(typeof(MethodInvoker));

                    _methodInvoker = (IMethodInvoker)ActivatorUtilities.CreateInstance(serviceProvider, proxyType, target, mapping);
                });

            _methodInvoker = null!;
            _publisher = null!;
        }

        public static IActorRef CreateHost(IActorRefFactory factory, object service, Type serviceType, IServiceProvider serviceProvider, string? name = null)
        {
            var map = service.GetType().GetInterfaceMap(serviceType);

            var actor = factory.ActorOf(Props.Create<ServiceHostActor>(), name);
            actor.Tell(new Init(service, map, serviceProvider));

            return actor;
        }

        private async Task<PublicationResponse> RunPublication(RequestPublication arg)
        {
            try
            {
                var publication = await _publisher
                   .Publish(_ => _methodInvoker.TryInvoke(arg.Call))
                   .ConfigureAwait(false);


                using (publication.Use())
                {
                    return new PublicationResponse(
                        publication.State.Computed.Value,
                        new PublicationStateInfo(publication.Ref, publication.State.Computed.Version, publication.State.Computed.IsConsistent()));
                }
            }
            catch (Exception e)
            {
                return new PublicationResponse(new MethodResponse(e, true), null);
            }
        }

        private sealed record Init(object Servive, InterfaceMapping Mapping, IServiceProvider ServiceProvider);

        private interface IMethodInvoker
        {
            [ComputeMethod]
            Task<MethodResponse> TryInvoke(TryMethodCall call);
        }

        private sealed class MethodInvoker : IMethodInvoker
        {
            private readonly InterfaceMapping _mapping;
            private readonly object _target;

            public MethodInvoker(object target, InterfaceMapping mapping)
            {
                _target = target;
                _mapping = mapping;
            }

            public async Task<MethodResponse> TryInvoke(TryMethodCall call)
            {
                try
                {
                    for (var i = 0; i < _mapping.InterfaceMethods.Length; i++)
                    {
                        var (name, aguments) = call;

                        if (_mapping.InterfaceMethods[i].Name != name) continue;

                        var m = _mapping.TargetMethods[i];

                        var caller = FastReflection.Shared.GetMethodInvoker(m, () => m.GetParameterTypes());

                        switch (caller(_target, aguments))
                        {
                            case Task genericTask when genericTask.GetType().IsGenericType:
                                return await AwaitWithResult(genericTask);
                            case Task task:
                                await task;

                                return new MethodResponse(null, false);
                            case { } result:
                                return new MethodResponse(result, false);
                            default:
                                return new MethodResponse(new NullReferenceException("Method return an Null which is not Supported"), true);
                        }
                    }

                    #pragma warning disable EX006
                    throw new MissingMethodException(_target.GetType().Name, call.Name);
                    #pragma warning restore EX006
                }
                catch (Exception e)
                {
                    return new MethodResponse(e, true);
                }
            }

            private static async Task<MethodResponse> AwaitWithResult(Task resultTask)
            {
                try
                {
                    await resultTask.ConfigureAwait(false);
                    var result = (object)((dynamic)resultTask).Result;

                    return new MethodResponse(result, false);
                }
                catch (Exception e)
                {
                    return new MethodResponse(e, true);
                }
            }
        }
    }
}