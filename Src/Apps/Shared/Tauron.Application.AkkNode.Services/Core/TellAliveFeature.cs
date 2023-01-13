using System;
using System.Reactive.Linq;
using Akka.Actor;
using Tauron.Features;

namespace Tauron.Application.AkkaNode.Services.Core;

public sealed class TellAliveFeature : ActorFeatureBase<EmptyState>
{
    public static IPreparedFeature New()
        => Feature.Create(() => new TellAliveFeature());

    protected override void ConfigImpl()
    {
        Receive<QueryIsAlive>(
            obs => (from ob in obs
                    from ident in ob.Sender.Ask<ActorIdentity>(new Identify(messageId: null), TimeSpan.FromSeconds(10))
                    select (ob.Sender, Response: new IsAliveResponse(IsAlive: true)))
               .AutoSubscribe(d => d.Sender.Tell(d.Response)));
    }
}