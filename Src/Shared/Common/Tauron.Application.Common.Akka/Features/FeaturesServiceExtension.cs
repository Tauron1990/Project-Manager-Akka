using System.Reflection;
using Akka.Actor;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Tauron.Features;

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

    protected FeatureActorRefBase(string? name)
        => _name = name;

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
}

[PublicAPI]
public static class FeaturesServiceExtension
{
    public static IServiceCollection RegisterFeature<TImpl, TInterfaceType>(this IServiceCollection builder, Delegate del)
        where TInterfaceType : class, IFeatureActorRef<TInterfaceType>
        where TImpl : TInterfaceType
    {
        var param = del.Method.GetParameters().Select(info => info.ParameterType).ToArray();
        var factory = ActivatorUtilities.CreateFactory(typeof(TImpl), GetParameters(typeof(TImpl)));

        return builder.AddSingleton<TInterfaceType>(
            s =>
            {
                var inst = (TImpl)factory(s, null);
                var system = s.GetRequiredService<ActorSystem>();
                
                inst.Init(
                    system,
                    () => del.DynamicInvoke(param.Select(s.GetService).ToArray()) switch
                    {
                        IPreparedFeature feature => Feature.Props(feature),
                        IPreparedFeature[] features => Feature.Props(features),
                        IEnumerable<IPreparedFeature> features => Feature.Props(features.ToArray()),
                        _ => throw new InvalidOperationException("Invalid Feature Construction Method")
                    });
                    
                return inst;
            });
    }

    private static Type[] GetParameters(Type type)
    {
        var contructors = type.GetConstructors();

        if (contructors.Length == 0)
            throw new InvalidOperationException($"No Public Constrctor for {type.Name} Defined");

        var constructor = contructors.FirstOrDefault(c => c.IsDefined(typeof(ActivatorUtilitiesConstructorAttribute))) ?? contructors.Single();

        return constructor.GetParameterTypes().ToArray();
    }
}