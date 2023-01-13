using Microsoft.Extensions.DependencyInjection;
using Tauron.TextAdventure.Engine.GamePackages.Core;
using Tauron.TextAdventure.Engine.Systems;

namespace Tauron.TextAdventure.Engine.GamePackages.Elements;

public sealed class SystemElement<TSystem> : PackageElement where TSystem : class, ISystem
{
    private static void Apply(IServiceCollection serviceCollection)
        => serviceCollection.AddTransient<ISystem, TSystem>();

    internal override void Load(ElementLoadContext context)
        => context.ConfigServices.Add(Apply);
}