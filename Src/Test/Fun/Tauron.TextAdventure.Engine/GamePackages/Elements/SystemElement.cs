using Microsoft.Extensions.DependencyInjection;
using Tauron.TextAdventure.Engine.Systems;

namespace Tauron.TextAdventure.Engine.GamePackages.Elements;

public sealed class SystemElement<TSystem> : PackageElement where TSystem : class, ISystem
{
    internal override void Apply(IServiceCollection serviceCollection)
        => serviceCollection.AddTransient<ISystem, TSystem>();

    internal override void PostConfig(IServiceProvider serviceProvider) { }
}