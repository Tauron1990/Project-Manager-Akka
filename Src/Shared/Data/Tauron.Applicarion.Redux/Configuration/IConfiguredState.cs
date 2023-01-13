using JetBrains.Annotations;

namespace Tauron.Applicarion.Redux.Configuration;

[PublicAPI]
public interface IConfiguredState
{
    void RunConfig(IRootStore store);

    void PostBuild(IRootStore store);
}