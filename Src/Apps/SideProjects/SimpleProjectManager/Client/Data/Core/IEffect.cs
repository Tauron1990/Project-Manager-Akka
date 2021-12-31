using ReduxSimple;

namespace SimpleProjectManager.Client.Data.Core;

public interface IEffect
{
    Effect<ApplicationState> Build();
}