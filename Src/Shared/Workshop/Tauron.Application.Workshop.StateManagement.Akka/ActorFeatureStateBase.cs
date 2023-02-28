using System.Reactive.Linq;
using JetBrains.Annotations;
using Tauron.Application.Workshop.StateManagement.Akka.Internal;
using Tauron.Features;

namespace Tauron.Application.Workshop.StateManagement.Akka;

[PublicAPI]
public abstract class ActorFeatureStateBase<TState> : ActorFeatureBase<TState>
{
    protected override void Config()
    {
        Observ<StateActorMessage>(obs => obs.Select(m => m.Event).SubscribeWithStatus(m => m.Apply(this)));

        base.Config();
    }
}