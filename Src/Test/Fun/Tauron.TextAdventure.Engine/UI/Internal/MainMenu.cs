using Microsoft.Extensions.Hosting;
using Tauron.TextAdventure.Engine.Systems;
using Tauron.TextAdventure.Engine.UI.Rendering;

namespace Tauron.TextAdventure.Engine.UI.Internal;

public sealed class MainMenu
{
    private readonly IUILayer _uiLayer;
    private readonly IEnumerable<ISystem> _systems;
    private readonly EventManager _eventManager;
    private readonly IRenderVisitor _renderVisitor;
    
    public MainMenu(IUILayer uiLayer, IEnumerable<ISystem> systems, EventManager eventManager)
    {
        _uiLayer = uiLayer;
        _systems = systems;
        _eventManager = eventManager;
        _renderVisitor = uiLayer.CreateForPage();
    }

    public async ValueTask Show()
    {
        RenderElement titleElement = _uiLayer.CreateTitle();
        RenderElement menuRender = MultiElement.Create(
                titleElement,
                new SpacingElement { Amount = 3 },
                new CommandItem(UiKeys.NewGame),
                new CommandItem(UiKeys.LoadGame),
                new CommandItem(UiKeys.CloseGame))
           .WithTag(Tags.MainMenu);

        while (true)
        {
            _renderVisitor.Visit(menuRender);
            string result = await _uiLayer.ExecutePage(_renderVisitor).ConfigureAwait(false);

            switch (result)
            {
                case UiKeys.NewGame:
                    string? name = await new NewGameMenu(_uiLayer).SelectName();
                    if(string.IsNullOrWhiteSpace(name)) continue;
                    
                    await GameHost.RunGame(name, _eventManager, _systems);
                    break;
                case UiKeys.LoadGame:
                    string? toLoad = await new LoadGameMenu(_uiLayer).Show();
                    if(string.IsNullOrWhiteSpace(toLoad)) continue;

                    await GameHost.RunGame(toLoad, _eventManager, _systems);
                    break;
                case UiKeys.CloseGame:
                    return;
            }
        }
    }
}