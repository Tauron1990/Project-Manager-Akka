using Akka.Actor;

namespace Tauron.Features;

public record struct SuperviserData(string Name, SupervisorStrategy? SupervisorStrategy)
{
    public static readonly SuperviserData DefaultSuperviser = new("DefaultSuperviser", SupervisorStrategy.DefaultStrategy);
}