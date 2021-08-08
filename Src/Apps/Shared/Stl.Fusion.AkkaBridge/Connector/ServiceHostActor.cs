using System;
using System.Reflection;
using System.Threading.Tasks;
using Akka.Actor;
using Tauron;

namespace Stl.Fusion.AkkaBridge.Connector
{
    public sealed class ServiceHostActor : ReceiveActor
    {
        public static IActorRef CreateHost(IActorRefFactory factory, object service, Type serviceType, string? name = null)
        {
            var map = service.GetType().GetInterfaceMap(serviceType);
            
            var actor = factory.ActorOf(Props.Create<ServiceHostActor>(), name);
            actor.Tell(new Init(service, map));

            return actor;
        }
        
        private object _target;
        private InterfaceMapping _mapping;

        public ServiceHostActor()
        {
            Receive<TryMethodCall>(RunMethodCall);
            Receive<Init>(
                i =>
                {
                    _target = i.Servive;
                    _mapping = i.Mapping;
                });
            _target = new object();
        }
        

        private void RunMethodCall(TryMethodCall obj)
        {
            try
            {
                for (int i = 0; i < _mapping.InterfaceMethods.Length; i++)
                {
                    var (name, aguments) = obj;

                    if(_mapping.InterfaceMethods[i].Name != name) continue;

                    var m = _mapping.TargetMethods[i];

                    var caller = FastReflection.Shared.GetMethodInvoker(m, () => m.GetParameterTypes());

                    switch (caller(_target, aguments))
                    {
                        case Task genericTask when genericTask.GetType().IsGenericType:
                            AwaitWithResult(genericTask).PipeTo(Sender, Self);
                            break;
                        case Task task:
                            task.ContinueWith(
                                t =>
                                {
                                    try
                                    {
                                        t.GetAwaiter().GetResult();

                                        return new MethodResponse(null, false);
                                    }
                                    catch (Exception e)
                                    {
                                        return new MethodResponse(e, true);
                                    }
                                }).PipeTo(Sender, Self, failure:e => new MethodResponse(e, true));
                            break;
                        case { } result:
                            Sender.Tell(new MethodResponse(result, false));
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Sender.Tell(new MethodResponse(e, true));
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

        private sealed record Init(object Servive, InterfaceMapping Mapping);
    }
}