using Microsoft.Extensions.DependencyInjection;
using Tauron.TextAdventure.Engine.Data;
using Tauron.TextAdventure.Engine.Systems;

namespace Tauron.TextAdventure.Engine.GamePackages.Elements;

public sealed class RegisterEvent<TEvent> : PackageElement
    where TEvent : IEvent
{
    private readonly Action<GameState, TEvent> _apply;

    public RegisterEvent(Action<GameState, TEvent> apply)
        => _apply = apply;

    internal override void Apply(IServiceCollection serviceCollection)
    {
        
    }

    internal override void PostConfig(IServiceProvider serviceProvider)
        => serviceProvider.GetRequiredService<EventManager>().RegisterEvent(_apply);
}