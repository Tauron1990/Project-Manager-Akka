using JetBrains.Annotations;
using Tauron.Applicarion.Redux.Extensions.Internal;

namespace Tauron.Applicarion.Redux.Configuration;

[PublicAPI]
public interface IConfiguredState
{
    void RunConfig(IReduxStore<MultiState> store, Action<Type, Guid> registerState);

    void PostBuild(IRootStore store);
}