using Tauron.TextAdventure.Engine.UI;

namespace Tauron.TextAdventure.Engine;

public abstract class GameBase
{
    protected internal abstract IUILayer CreateUILayer(IServiceProvider serviceProvider);

    internal async ValueTask Run(CancellationToken token)
    {
        
    }
}