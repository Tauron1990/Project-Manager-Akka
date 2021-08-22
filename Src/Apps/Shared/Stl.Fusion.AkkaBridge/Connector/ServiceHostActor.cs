using System;
using System.ComponentModel.Design;
using System.Reflection;
using System.Threading.Tasks;
using Akka.Actor;
using Microsoft.Extensions.DependencyInjection;
using Stl.DependencyInjection;
using Stl.Fusion.Bridge;
using Stl.Fusion.Interception;
using Tauron;

namespace Stl.Fusion.AkkaBridge.Connector
{
    public sealed class ServiceHostActor : ReceiveActor
    {
        public static IActorRef CreateHost(IActorRefFactory factory, object service, Type serviceType, IServiceProvider serviceProvider, string? name = null)
        {
            var map = service.GetType().GetInterfaceMap(serviceType);
            
            var actor = factory.ActorOf(Props.Create<ServiceHostActor>(), name);
            actor.Tell(new Init(service, map, serviceProvider));

            return actor;
        }
        
        private IPublisher     _publisher;
        private IMethodInvoker _methodInvoker;
        
        public ServiceHostActor()
        {
            Receive<RequestPublication>(p => RunPublication(p).PipeTo(Sender));
            Receive<TryMethodCall>(m => _methodInvoker!.TryInvoke(m).PipeTo(Sender));
            Receive<Init>(
                i =>
                {
                    var (target, mapping, serviceProvider) = i;
                    _publisher                             = serviceProvider.GetRequiredService<IPublisher>();
                    var proxyGenerator = serviceProvider.GetRequiredService<IComputeServiceProxyGenerator>();
                    var proxyType      = proxyGenerator.GetProxyType(typeof(MethodInvoker));

                    _methodInvoker = (IMethodInvoker)ActivatorUtilities.CreateInstance(serviceProvider, proxyType, target, mapping);
                });

            _methodInvoker = null!;
            _publisher     = null!;
        }

        protected override void PreRestart(Exception reason, object message)
        {
            base.PreRestart(reason, message);
        }

        protected override void PostStop()
        {
            base.PostStop();
        }

        private async Task<PublicationResponse> RunPublication(RequestPublication arg)
        {
            try
            {
                var publication = await _publisher
                                       .Publish(_ => _methodInvoker.TryInvoke(arg.Call))
                                       .ConfigureAwait(false);


                using (publication.Use())
                    return new PublicationResponse(
                        publication.State.Computed.Value,
                        new PublicationStateInfo(publication.Ref, publication.State.Computed.Version, publication.State.Computed.IsConsistent()));
            }
            catch (Exception e)
            {
                return new PublicationResponse(new MethodResponse(e, true), null);
            }
        }

        private sealed record Init(object Servive, InterfaceMapping Mapping, IServiceProvider ServiceProvider);
        
        public interface IMethodInvoker
        {
            [ComputeMethod]
            Task<MethodResponse> TryInvoke(TryMethodCall call);
        }
        
        public class MethodInvoker : IMethodInvoker
        {
            private object           _target;
            private InterfaceMapping _mapping;

            public MethodInvoker(object target, InterfaceMapping mapping)
            {
                _target  = target;
                _mapping = mapping;
            }

            public virtual async Task<MethodResponse> TryInvoke(TryMethodCall call)
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

                    throw new MissingMethodException(_target.GetType().Name, call.Name);
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