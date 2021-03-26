using System;
using System.Reflection;
using Akka.Actor;
using Akka.DependencyInjection;
using Autofac;
using JetBrains.Annotations;

namespace Tauron
{
    [PublicAPI]
    public static class AutofacExtensions
    {
        public static readonly MethodInfo ServiceProviderPropsMethod = Reflex.MethodInfo<ServiceProvider>(provider => provider.Props<ActorBase>()).GetGenericMethodDefinition();

        public static Props Props(this ServiceProvider provider, Type target, params object[] args) 
            => ((Func<object[], Props>)Delegate.CreateDelegate(typeof(Func<object[], Props>),provider, ServiceProviderPropsMethod.MakeGenericMethod(target)))(args);

        public static void WhenNotRegistered<TService>(this ContainerBuilder builder, Action<ContainerBuilder> register)
        {
            var startableType = typeof(TService);
            var startableTypeKey = $"PreventDuplicateRegistration({startableType.FullName})";

            if (builder.Properties.ContainsKey(startableTypeKey))
                return;

            builder.Properties.Add(startableTypeKey, null);
            register(builder);
        }
    }
}