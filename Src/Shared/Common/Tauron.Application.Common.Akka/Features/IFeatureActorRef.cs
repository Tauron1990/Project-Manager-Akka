using Akka.Actor;
using JetBrains.Annotations;

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