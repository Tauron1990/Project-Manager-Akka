using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SpaceConqueror.UI;
using Tauron.TextAdventure.Engine;
using Tauron.TextAdventure.Engine.GamePackages;
using Tauron.TextAdventure.Engine.UI;

namespace SpaceConqueror;

public sealed class Game : GameBase
{
    protected override IUILayer CreateUILayer(IServiceProvider serviceProvider)
        => (ConsoleUI)ActivatorUtilities.CreateInstance(serviceProvider, typeof(ConsoleUI));

    protected override IGamePackageFetcher CreateGamePackage()
        => new DefaultPackageLoader();

    protected override void ConfigurateHost(IHostBuilder builder)
    {
        base.ConfigurateHost(builder);

        builder.ConfigureServices(s => s.RemoveAll<ILoggerProvider>());
    }
}