using JetBrains.Annotations;
using ReduxSimple;

namespace SimpleProjectManager.Client.Data.Core;

[PublicAPI]
public interface IConfiguredState
{
    void RunConfig(ReduxStore<ApplicationState> store, Action<Type, Guid> registerState);

    void PostBuild(IRootStore store);
}