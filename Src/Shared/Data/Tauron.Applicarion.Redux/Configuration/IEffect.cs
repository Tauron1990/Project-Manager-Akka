using Tauron.Applicarion.Redux.Extensions.Internal;
using Tauron.Applicarion.Redux.Internal;

namespace Tauron.Applicarion.Redux.Configuration;

public interface IEffect
{
    Effect<MultiState> Build();
}