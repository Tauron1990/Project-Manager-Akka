using System;
using System.Reflection;
using System.Threading.Tasks;
using Akka.Actor;
using Tauron;

namespace Stl.Fusion.AkkaBridge.Connector
{
    public sealed class ServiceHostActor : ReceiveActor
    {
        public static Props CreateHost(object service, Type serviceType)
        {
            var map = service.GetType().GetInterfaceMap(serviceType);
            
            return Props.Create(() => new ServiceHostActor(service, map));
        }
        
        private readonly object _target;
        private readonly InterfaceMapping _mapping;

        private ServiceHostActor(object target, InterfaceMapping mapping)
        {
            _target = target;
            _mapping = mapping;
            Receive<TryMethodCall>(RunMethodCall);
        }

        private void RunMethodCall(TryMethodCall obj)
        {
            try
            {
                for (int i = 0; i < _mapping.InterfaceMethods.Length; i++)
                {
                    var (name, aguments) = obj;

                    if(_mapping.InterfaceMethods[i].Name != name) return;

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
    }
}