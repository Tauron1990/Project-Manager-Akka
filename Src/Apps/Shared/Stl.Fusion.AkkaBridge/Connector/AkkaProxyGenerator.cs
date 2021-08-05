using System;
using System.Reflection;
using System.Threading.Tasks;
using Akka.Actor;
using Castle.Core;
using Castle.DynamicProxy;
using Tauron;

namespace Stl.Fusion.AkkaBridge.Connector
{
    public class AkkaProxyGenerator
    {
        private readonly ActorSystem    _system;
        private readonly ProxyGenerator _proxyGenerator;
        
        public AkkaProxyGenerator(ActorSystem system)
        {
            _system         = system;
            _proxyGenerator = new ProxyGenerator();
        }

        public object GenerateAkkaProxy(Type serviceType)
            => _proxyGenerator.CreateInterfaceProxyWithoutTarget(serviceType, new AkkaInterceptor(_system));
        
        private sealed class AkkaInterceptor : IInterceptor
        {
            private static readonly MethodInfo _callServiceMethod =
                Reflex.MethodInfo<AkkaInterceptor>(t => t.CallService<string>(null!))
                   .GetGenericMethodDefinition();
            
            private readonly ActorSystem _system;

            public AkkaInterceptor(ActorSystem system)
                => _system = system;

            public void Intercept(IInvocation invocation)
            {
                var method = invocation.MethodInvocationTarget.ReturnType == typeof(Task)
                                 ? _callServiceMethod.MakeGenericMethod(typeof(EmptyTaskMarker))
                                 : _callServiceMethod.MakeGenericMethod(invocation.MethodInvocationTarget.ReturnType.GenericTypeArguments[0]);

                invocation.ReturnValue = FastReflection.Shared
                   .GetMethodInvoker(method, invocation.MethodInvocationTarget.GetParameterTypes)(this, new object?[]{ invocation });
            }

            private Task CallService<TType>(IInvocation invocation)
            {
                
            }
        }

        private sealed record EmptyTaskMarker;
    }
}