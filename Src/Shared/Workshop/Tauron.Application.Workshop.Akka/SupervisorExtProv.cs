using Akka.Actor;

namespace Tauron.Application.Workshop;

internal class SupervisorExtProv : ExtensionIdProvider<SuprvisorExt>
{
    internal static SupervisorExtProv Inst = new();

    public override SuprvisorExt CreateExtension(ExtendedActorSystem system) => new(system);
}