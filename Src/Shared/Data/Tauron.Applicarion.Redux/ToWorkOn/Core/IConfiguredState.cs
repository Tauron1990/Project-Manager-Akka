using JetBrains.Annotations;
using ReduxSimple;
using Tauron.Applicarion.Redux.Extensions.Internal;

namespace SimpleProjectManager.Client.Data.Core;

[PublicAPI]
public interface IConfiguredState
{
    void RunConfig(ReduxStore<MultiState> store, Action<Type, Guid> registerState);

    void PostBuild(IRootStore store);
}