using Microsoft.Extensions.Hosting;
using SpaceConqueror.UI;
using Tauron.TextAdventure.Engine;
using Tauron.TextAdventure.Engine.GamePackages;
using Tauron.TextAdventure.Engine.UI;

namespace SpaceConqueror;

public sealed class Game : GameBase
{
    protected override IUILayer CreateUILayer()
        => new ConsoleUI();

    protected override IGamePackageFetcher CreateGamePackage()
        => new DefaultPackageLoader();

    protected override void ConfigurateHost(IHostBuilder builder)
        => builder.UseConsoleLifetime();
}