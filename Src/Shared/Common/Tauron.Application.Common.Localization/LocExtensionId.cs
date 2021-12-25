using Akka.Actor;
using Tauron.Application.Common.Localization.Extension;

namespace Tauron.Application.Common.Localization;

public sealed class LocExtensionId : ExtensionIdProvider<LocExtension>
{
    public override LocExtension CreateExtension(ExtendedActorSystem system) => new LocExtension().Init(system);
}