using Akka.Actor;
using Akka.Hosting;
using Tauron.Features;

namespace SimpleProjectManager.Client.Operations.Shared;

public sealed class NameRegistry : FeatureActorRefBase<NameRegistry>
{
    public NameRegistry(IRequiredActor<NameRegistry> actor) : base(actor)
    {
    }
}