using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tauron.TextAdventure.Engine.Core;
using Tauron.TextAdventure.Engine.GamePackages;
using Tauron.TextAdventure.Engine.UI;
using Tauron.TextAdventure.Engine.UI.Internal;

namespace Tauron.TextAdventure.Engine;

[PublicAPI]
public abstract class GameBase
{
    protected internal abstract IUILayer CreateUILayer(IServiceProvider serviceProvider);

    protected internal abstract IGamePackageFetcher CreateGamePackage();

    protected internal virtual void ConfigurateHost(IHostBuilder builder) { }
    
    internal async ValueTask Run(IServiceProvider serviceProvider, CancellationToken token)
        => await serviceProvider.GetRequiredService<MainMenu>().Show().ConfigureAwait(false);
}