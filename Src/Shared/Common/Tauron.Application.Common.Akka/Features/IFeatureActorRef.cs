using Akka.Actor;
using JetBrains.Annotations;

namespace Tauron.Features;

[PublicAPI]
public interface IFeatureActorRef<out TInterface> : ICanTell
    where TInterface : IFeatureActorRef<TInterface>
{
    IActorRef Actor { get; }
    
    TInterface Tell(object msg);

    TInterface Forward(object msg);

    Task<TResult> Ask<TResult>(object msg, TimeSpan? timeout = null);
}