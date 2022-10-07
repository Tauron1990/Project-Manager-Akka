using System.Reflection;
using Akka.Actor;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Tauron.Features;

[PublicAPI]
public interface IFeatureActorRef<out TInterface> : ICanTell
    where TInterface : IFeatureActorRef<TInterface>
{
    Task<IActorRef> Actor { get; }

    void Init(Func<Props, Task<IActorRef>> factory, Func<Props> resolver);

    TInterface Tell(object msg);

    TInterface Forward(object msg);

    Task<TResult> Ask<TResult>(object msg, TimeSpan? timeout = null);
}

[PublicAPI]
public abstract class FeatureActorRefBase<TInterface> : IFeatureActorRef<TInterface>
    where TInterface : IFeatureActorRef<TInterface>
{
    private readonly TaskCompletionSource<IActorRef> _actorSource = new();

    public Task<IActorRef> Actor => _actorSource.Task;

    async void IFeatureActorRef<TInterface>.Init(Func<Props, Task<IActorRef>> factoryTask, Func<Props> resolver)
    {
        if(_actorSource.Task.IsCompleted)
            throw new InvalidOperationException("Initialization of Actor Compleded");
        
        try
        {
            var task = factoryTask(resolver());
            if(await Task.WhenAny(task, Task.Delay(TimeSpan.FromSeconds(20))).ConfigureAwait(false) == task)
            {
                _actorSource.TrySetResult(await task.ConfigureAwait(false));
            }
            else
                _actorSource.TrySetCanceled();
        }
        catch (Exception e)
        {
            _actorSource.TrySetException(e);
        }
    }

    private void OnActor(Action<IActorRef> runner)
    {
        if(Actor.IsCompletedSuccessfully)
            runner(Actor.Result);
        else if(Actor.IsCanceled)
            throw new TaskCanceledException(Actor);
        else if(Actor.IsFaulted)
            throw Actor.Exception ?? throw new InvalidOperationException("Unkown Error on executing ActorTask");
        else
            Actor.ContinueWith(
                t =>
                {
                    if(t.IsCompletedSuccessfully)
                        runner(t.Result);
                });
    }
    
    public TInterface Tell(object msg)
    {
       OnActor(a => a.Tell(msg));

        return (TInterface)(object)this;
    }

    public TInterface Forward(object msg)
    {
        OnActor(a => a.Forward(msg));

        return (TInterface)(object)this;
    }

    public async Task<TResult> Ask<TResult>(object msg, TimeSpan? timeout = null)
    {
        var actor = await Actor.ConfigureAwait(false);

        return await actor.Ask<TResult>(msg, timeout).ConfigureAwait(false);
    }

    public void Tell(object message, IActorRef sender)
        => OnActor(a => a.Tell(message, sender));
}

public record struct SuperviserData(string Name, SupervisorStrategy? SupervisorStrategy)
{
    public static readonly SuperviserData DefaultSuperviser = new("DefaultSuperviser", SupervisorStrategy.DefaultStrategy);
}

[PublicAPI]
public static class FeaturesServiceExtension
{
    public static IServiceCollection RegisterFeature<TInterfaceType>(this IServiceCollection builder, Delegate del)
        where TInterfaceType : class, IFeatureActorRef<TInterfaceType>
        => RegisterFeature<TInterfaceType, TInterfaceType>(builder, del, null, null);
    
    public static IServiceCollection RegisterFeature<TInterfaceType>(this IServiceCollection builder, Delegate del,
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
    
    public static IServiceCollection RegisterFeature<TImpl, TInterfaceType>(this IServiceCollection builder, Delegate del,
        string? actorName, Func<SuperviserData>? supervisorStrategy)
        where TInterfaceType : class, IFeatureActorRef<TInterfaceType>
        where TImpl : TInterfaceType
    {
        var param = del.Method.GetParameters().Select(info => info.ParameterType).ToArray();
        var factory = ActivatorUtilities.CreateFactory(typeof(TImpl), GetParameters(typeof(TImpl)));

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
                        _ => throw new InvalidOperationException("Invalid Feature Construction Method")
                    });
                    
                return inst;
                
                async Task<IActorRef> CreateActor(Props props)
                {
                    var system = (ExtendedActorSystem)s.GetRequiredService<ActorSystem>();

                    if(supervisorStrategy is null) return system.ActorOf(props);

                    var data = supervisorStrategy();
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

                    var result = await supervisor
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

        if (contructors.Length == 0)
            throw new InvalidOperationException($"No Public Constrctor for {type.Name} Defined");

        var constructor = contructors.FirstOrDefault(c => c.IsDefined(typeof(ActivatorUtilitiesConstructorAttribute))) ?? contructors.Single();

        return constructor.GetParameterTypes().ToArray();
    }
}