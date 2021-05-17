using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using Autofac;
using Autofac.Builder;
using JetBrains.Annotations;

namespace Tauron.Features
{
    public interface IFeatureActorRef<out TInterface>
        where TInterface : IFeatureActorRef<TInterface>
    {
        IActorRef Actor { get; }

        void Init(ActorSystem system, Func<Props> resolver);

        TInterface Tell(object msg);

        TInterface Forward(object msg);

        Task<TResult> Ask<TResult>(object msg, TimeSpan? timeout = null);
    }

    [PublicAPI]
    public abstract class FeatureActorRefBase<TInterface> : IFeatureActorRef<TInterface> 
        where TInterface : IFeatureActorRef<TInterface>
    {
        private readonly string? _name;

        public IActorRef Actor { get; private set; } = ActorRefs.Nobody;

        void IFeatureActorRef<TInterface>.Init(ActorSystem system, Func<Props> resolver) 
            => Actor = system.ActorOf(resolver(), _name);

        public TInterface Tell(object msg)
        {
            Actor.Tell(msg);
            return (TInterface)(object)this;
        }

        public TInterface Forward(object msg)
        {
            Actor.Forward(msg);
            return (TInterface)(object)this;
        }

        public Task<TResult> Ask<TResult>(object msg, TimeSpan? timeout = null) 
            => Actor.Ask<TResult>(msg, timeout);

        protected FeatureActorRefBase(string? name) 
            => _name = name;
        
    }

    [PublicAPI]
    public static class FeaturAutofacExtension
    {
        public static IRegistrationBuilder<TImpl, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterFeature<TImpl, TIterfaceType>(this ContainerBuilder builder, Delegate del)
            where TIterfaceType : IFeatureActorRef<TIterfaceType>
            where TImpl : TIterfaceType
        {
            var param = del.Method.GetParameters().Select(p => p.ParameterType).ToArray();

            return builder.RegisterType<TImpl>()
                          .OnActivated(e =>
                                       {
                                           var system = e.Context.Resolve<ActorSystem>();
                                           e.Instance.Init(system,
                                               () => del.DynamicInvoke(param.Select(e.Context.Resolve).ToArray()) switch
                                               {
                                                   IPreparedFeature feature => Feature.Props(feature),
                                                   IPreparedFeature[] features => Feature.Props(features),
                                                   IEnumerable<IPreparedFeature> features => Feature.Props(features.ToArray()),
                                                   _ => throw new InvalidOperationException("Invalid Feature Construction Metod")
                                               });
                                       })
                          .As<TIterfaceType>().SingleInstance();
        }
    }
}