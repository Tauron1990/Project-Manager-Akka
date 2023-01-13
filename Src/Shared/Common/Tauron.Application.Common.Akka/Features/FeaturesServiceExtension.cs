using System.Reflection;
using Akka.Actor;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Tauron.Features;

[PublicAPI]
public static class FeaturesServiceExtension
{
    public static IServiceCollection RegisterFeature<TInterfaceType>(this IServiceCollection builder, Delegate del)
        where TInterfaceType : class, IFeatureActorRef<TInterfaceType>
        => RegisterFeature<TInterfaceType, TInterfaceType>(builder, del, null, null);

    public static IServiceCollection RegisterFeature<TInterfaceType>(
        this IServiceCollection builder, Delegate del,
        string? actorName)
        where TInterfaceType : class, IFeatureActorRef<TInterfaceType>
        => RegisterFeature<TInterfaceType, TInterfaceType>(builder, del, actorName, null);

    public static IServiceCollection RegisterFeature<TInterfaceType>(this IServiceCollection builder, Delegate del, Func<SuperviserData>? supervisorStrategy)
        where TInterfaceType : class, IFeatureActorRef<TInterfaceType>
        => RegisterFeature<TInterfaceType, TInterfaceType>(builder, del, null, supervisorStrategy);

    public static IServiceCollection RegisterFeature<TInterfaceType>(this IServiceCollection builder, Delegate del, string? actorName, Func<SuperviserData>? supervisorStrategy)
        where TInterfaceType : class, IFeatureActorRef<TInterfaceType>
        => RegisterFeature<TInterfaceType, TInterfaceType>(builder, del, actorName, supervisorStrategy);

    public static IServiceCollection RegisterFeature<TImpl, TInterfaceType>(
        this IServiceCollection builder, Delegate del)
        where TInterfaceType : class, IFeatureActorRef<TInterfaceType>
        where TImpl : TInterfaceType
        => RegisterFeature<TImpl, TInterfaceType>(builder, del, null, null);

    public static IServiceCollection RegisterFeature<TImpl, TInterfaceType>(
        this IServiceCollection builder, Delegate del,
        Func<SuperviserData>? supervisorStrategy)
        where TInterfaceType : class, IFeatureActorRef<TInterfaceType>
        where TImpl : TInterfaceType
        => RegisterFeature<TImpl, TInterfaceType>(builder, del, null, supervisorStrategy);

    public static IServiceCollection RegisterFeature<TImpl, TInterfaceType>(
        this IServiceCollection builder, Delegate del,
        string? actorName)
        where TInterfaceType : class, IFeatureActorRef<TInterfaceType>
        where TImpl : TInterfaceType
        => RegisterFeature<TImpl, TInterfaceType>(builder, del, actorName, null);

    public static IServiceCollection RegisterFeature<TImpl, TInterfaceType>(
        this IServiceCollection builder, Delegate del,
        string? actorName, Func<SuperviserData>? supervisorStrategy)
        where TInterfaceType : class, IFeatureActorRef<TInterfaceType>
        where TImpl : TInterfaceType
    {
        var param = del.Method.GetParameters().Select(info => info.ParameterType).ToArray();
        ObjectFactory factory = ActivatorUtilities.CreateFactory(typeof(TImpl), GetParameters(typeof(TImpl)));

        return builder.AddSingleton<TInterfaceType>(
            s =>
            {
                var inst = (TImpl)factory(s, null);

                inst.Init(
                    CreateActor,
                    () => del.DynamicInvoke(param.Select(s.GetService).ToArray()) switch
                    {
                        IPreparedFeature feature => Feature.Props(feature),
                        IPreparedFeature[] features => Feature.Props(features),
                        IEnumerable<IPreparedFeature> features => Feature.Props(features.ToArray()),
                        _ => throw new InvalidOperationException("Invalid Feature Construction Method"),
                    });

                return inst;

                async Task<IActorRef> CreateActor(Props props)
                {
                    var system = (ExtendedActorSystem)s.GetRequiredService<ActorSystem>();

                    if(supervisorStrategy is null) return system.ActorOf(props);

                    SuperviserData data = supervisorStrategy();
                    var selector = new ActorSelection(system.Guardian, data.Name);
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
            });
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