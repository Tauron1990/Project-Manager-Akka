using JetBrains.Annotations;

namespace Tauron.Application.Akka.Redux.Configuration;

[PublicAPI]
public interface IConfiguredState
{
    void RunConfig(IRootStore store);

    void PostBuild(IRootStore store);
}