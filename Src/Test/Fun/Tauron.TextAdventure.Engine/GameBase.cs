using Tauron.TextAdventure.Engine.GamePackages;
using Tauron.TextAdventure.Engine.UI;

namespace Tauron.TextAdventure.Engine;

public abstract class GameBase
{
    protected internal abstract IUILayer CreateUILayer(IServiceProvider serviceProvider);

    protected internal abstract IGamePackageFetcher CreateGamePackage();
    
    internal async ValueTask Run(CancellationToken token)
    {
        
    }
}