using System;
using System.Reflection;
using System.Threading.Tasks;
using Akka.Actor;
using Stl.Fusion.Bridge;
using Tauron;

namespace Stl.Fusion.AkkaBridge.Connector
{
    public sealed class ServiceHostActor : ReceiveActor
    {
        public static IActorRef CreateHost(IActorRefFactory factory, object service, Type serviceType, IPublisher publisher, string? name = null)
        {
            var map = service.GetType().GetInterfaceMap(serviceType);
            
            var actor = factory.ActorOf(Props.Create<ServiceHostActor>(), name);
            actor.Tell(new Init(service, map, publisher));

            return actor;
        }
        
        private object _target;
        private InterfaceMapping _mapping;
        private IPublisher _publisher;

        public ServiceHostActor()
        {
            Receive<RequestPublication>(p => RunPublication(p).PipeTo(Sender));
            Receive<TryMethodCall>(m => RunMethodCall(m).PipeTo(Sender));
            Receive<Init>(
                i => (_target, _mapping, _publisher) = i);
            _target = new object();
            _publisher = null!;
        }

        public async Task<PublicationResponse> RunPublication(RequestPublication arg)
        {
            try
            {
                var publication = await _publisher
                                       .Publish(_ => RunMethodCall(arg.Call))
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

        private async Task<MethodResponse> RunMethodCall(TryMethodCall obj)
        {
            try
            {
                for (var i = 0; i < _mapping.InterfaceMethods.Length; i++)
                {
                    var (name, aguments) = obj;

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

                throw new MissingMethodException(_target.GetType().Name, obj.Name);
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

        private sealed record Init(object Servive, InterfaceMapping Mapping, IPublisher Publisher);
    }
}