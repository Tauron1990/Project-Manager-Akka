using Akka.Actor;

namespace Tauron.Features;

public interface ISimpleFeature
{
    Props MakeProps();
}