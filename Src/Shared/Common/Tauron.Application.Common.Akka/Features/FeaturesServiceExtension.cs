using System.Reflection;
using Akka.Actor;
using Akka.DependencyInjection;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Tauron.AkkaHost;

namespace Tauron.Features;

[PublicAPI]
public static class FeaturesServiceExtension
{
    public static IActorApplicationBuilder RegisterFeature<TInterfaceType>(
        this IActorApplicationBuilder builder, Delegate del,
        string? actorName)
        where TInterfaceType : class, IFeatureActorRef<TInterfaceType>
        => RegisterFeature<TInterfaceType>(builder, del, actorName, supervisorStrategy: null);

    public static IActorApplicationBuilder RegisterFeature<TInterfaceType>(this IActorApplicationBuilder builder, Delegate del, Func<SuperviserData>? supervisorStrategy)
        where TInterfaceType : class, IFeatureActorRef<TInterfaceType>
        => RegisterFeature<TInterfaceType>(builder, del, actorName: null, supervisorStrategy);

    public static IActorApplicationBuilder RegisterFeature<TInterfaceType>(
        this IActorApplicationBuilder builder, Delegate del)
        where TInterfaceType : class, IFeatureActorRef<TInterfaceType>
        => RegisterFeature<TInterfaceType>(builder, del, actorName: null, supervisorStrategy: null);

    public static IActorApplicationBuilder RegisterFeature<TInterfaceType>(
        this IActorApplicationBuilder builder, Delegate del,
        string? actorName, Func<SuperviserData>? supervisorStrategy)
        where TInterfaceType : class, IFeatureActorRef<TInterfaceType>
    {
        var param = del.Method.GetParameters().Select(info => info.ParameterType).ToArray();

        return builder
            .ConfigureServices(s => s.AddSingleton<TInterfaceType>())
            .StartActors(
                async (system, registry, resolver) =>
                {
                    Props props = CreateProps(resolver);

                    registry.Register<TInterfaceType>(await CreateActor(system, props).ConfigureAwait(false));
                });
        
        Props CreateProps(IDependencyResolver s)
        {
            return del.DynamicInvoke(param.Select(s.GetService).ToArray()) switch
            {
                IPreparedFeature feature => Feature.Props(feature),
                IPreparedFeature[] features => Feature.Props(features),
                IEnumerable<IPreparedFeature> features => Feature.Props(features.ToArray()),
                _ => throw new InvalidOperationException("Invalid Feature Construction Method"),
            };
        }
        
        async ValueTask<IActorRef> CreateActor(ActorSystem system, Props props)
        {
            if(supervisorStrategy is null) return system.ActorOf(props, actorName);

            SuperviserData data = supervisorStrategy();
            ActorSelection selector = system.ActorSelection($"/user/{data.Name}");
            IActorRef supervisor;
            try
            {
                supervisor = await selector.ResolveOne(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
            }
            catch (ActorNotFoundException)
            {
                supervisor = system.ActorOf(Props.Create(() => new GenericSupervisorActor(data.SupervisorStrategy)), data.Name);
            }

            GenericSupervisorActor.CreateActorResult? result = await supervisor
                .Ask<GenericSupervisorActor.CreateActorResult>(
                    new GenericSupervisorActor.CreateActor(actorName, props),
                    TimeSpan.FromSeconds(15))
                .ConfigureAwait(false);

            return result.Actor;
        }
    }

    private static Type[] GetParameters(Type type)
    {
        var contructors = type.GetConstructors();

        if(contructors.Length == 0)
            throw new InvalidOperationException($"No Public Constrctor for {type.Name} Defined");

        ConstructorInfo constructor = contructors.FirstOrDefault(c => c.IsDefined(typeof(ActivatorUtilitiesConstructorAttribute))) ?? contructors.Single();

        return constructor.GetParameterTypes().ToArray();
    }
}