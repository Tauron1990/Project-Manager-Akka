using Microsoft.Extensions.DependencyInjection;
using Tauron.TextAdventure.Engine.Data;
using Tauron.TextAdventure.Engine.GamePackages.Core;
using Tauron.TextAdventure.Engine.Systems;

namespace Tauron.TextAdventure.Engine.GamePackages.Elements;

public sealed class RegisterEvent<TEvent> : PackageElement
    where TEvent : IEvent
{
    private readonly Action<GameState, TEvent> _apply;

    public RegisterEvent(Action<GameState, TEvent> apply)
        => _apply = apply;

    private void PostConfig(IServiceProvider serviceProvider)
        => serviceProvider.GetRequiredService<EventManager>().RegisterEvent(_apply);

    internal override void Load(ElementLoadContext context)
        => context.PostConfigServices.Add(PostConfig);
}