using ReduxSimple;
using Tauron.Applicarion.Redux.Extensions.Internal;

namespace SimpleProjectManager.Client.Data.Core;

public interface IEffect
{
    Effect<MultiState> Build();
}