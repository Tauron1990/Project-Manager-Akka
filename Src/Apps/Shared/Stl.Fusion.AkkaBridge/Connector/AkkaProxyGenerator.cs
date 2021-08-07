using System;
using System.Reflection;
using System.Threading.Tasks;
using Akka.Actor;
using Castle.Core;
using Castle.DynamicProxy;
using JetBrains.Annotations;
using Tauron;

namespace Stl.Fusion.AkkaBridge.Connector
{
    public class AkkaProxyGenerator
    {
        private readonly ActorSystem    _system;
        private readonly IServiceRegistryActor _serviceRegistry;
        private readonly ProxyGenerator _proxyGenerator;
        
        public AkkaProxyGenerator(ActorSystem system, IServiceRegistryActor serviceRegistry)
        {
            _system         = system;
            _serviceRegistry = serviceRegistry;
            _proxyGenerator = new ProxyGenerator();
        }

        public object GenerateAkkaProxy(Type serviceType)
            => _proxyGenerator.CreateInterfaceProxyWithoutTarget(serviceType, new AkkaInterceptor(_system, _serviceRegistry, serviceType));
        
        private sealed class AkkaInterceptor : IInterceptor
        {
            private static readonly MethodInfo _callServiceMethod =
                Reflex.MethodInfo<AkkaInterceptor>(t => t.CallService<string>(null!))
                   .GetGenericMethodDefinition();
            
            private readonly ActorSystem _system;
            private readonly IServiceRegistryActor _serviceRegistry;
            private readonly Type _interfaceMethod;

            public AkkaInterceptor(ActorSystem system, IServiceRegistryActor serviceRegistry, Type interfaceMethod)
            {
                _system = system;
                _serviceRegistry = serviceRegistry;
                _interfaceMethod = interfaceMethod;
            }

            public void Intercept(IInvocation invocation)
            {
                var returnType = invocation.Method.ReturnType;

                if (!(returnType == typeof(Task) || (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))))
                    throw new InvalidOperationException("Only Async Operation are Supported");

                if (returnType == typeof(Task))
                    invocation.ReturnValue = CallServiceNoReturn(invocation);
                else
                {
                    var met = _callServiceMethod.MakeGenericMethod(returnType.GenericTypeArguments[0]);
                    invocation.ReturnValue = FastReflection.Shared
                                                         .GetMethodInvoker(met, () => new[] { typeof(IInvocation) })(this, new object?[]{ invocation });
                    
                }
            }

            [UsedImplicitly]
            private async Task<TType?> CallService<TType>(IInvocation invocation)
            {
                var response = await TryCall(invocation);

                return response is TType type ? type : default;
            }

            private async Task CallServiceNoReturn(IInvocation invocation)
                => await TryCall(invocation);

            private async Task<MethodResponse> TryCall(IInvocation invocation)
            {
                var response = await (await GetService()).Ask<MethodResponse>(new TryMethodCall(invocation.Method.Name, invocation.Arguments));

                return response.Error switch
                {
                    true when response.Response is Exception ex => throw ex,
                    true => throw new InvalidOperationException("Unkownen Error on Excution Remote Service"),
                    _ => response
                };
            }
            private async Task<IActorRef> GetService()
            {
                var (actorRef, exception) = await _serviceRegistry.ResolveService(new ResolveService(_interfaceMethod), TimeSpan.FromHours(10));

                if (!actorRef.IsNobody()) return actorRef;

                if (exception != null)
                    throw exception;
                throw new InvalidOperationException("Unkownen Error on Excution Remote Service");
            }
        }

        private sealed record EmptyTaskMarker;
    }
}