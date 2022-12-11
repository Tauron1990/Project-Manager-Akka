using Tauron.TextAdventure.Engine.UI.Rendering;

namespace Tauron.TextAdventure.Engine.UI.Internal;

public sealed class MainMenu
{
    private readonly IUILayer _uiLayer;
    private readonly IServiceProvider _serviceProvider;
    private readonly IRenderVisitor _renderVisitor;
    
    public MainMenu(IUILayer uiLayer, IServiceProvider serviceProvider)
    {
        _uiLayer = uiLayer;
        _serviceProvider = serviceProvider;
        _renderVisitor = uiLayer.CreateForPage();
    }

    // ReSharper disable once CognitiveComplexity
    public async ValueTask Show()
    {
        RenderElement menuRender = MultiElement.Create(
                _uiLayer.CreateTitle(),
                new SpacingElement { Amount = 3 },
                new CommandItem(UiKeys.NewGame),
                new CommandItem(UiKeys.LoadGame),
                new CommandItem(UiKeys.CloseGame))
           .WithTag(Tags.MainMenu);

        while (true)
        {
            _renderVisitor.Visit(menuRender);
            string? result = await _uiLayer.ExecutePage(_renderVisitor).ConfigureAwait(false);

            switch (result)
            {
                case UiKeys.NewGame:
                    string? name = await new NewGameMenu(_uiLayer).SelectName().ConfigureAwait(false);
                    if(string.IsNullOrWhiteSpace(name)) continue;
                    
                    await GameHost.RunGame(name, _serviceProvider).ConfigureAwait(false);
                    break;
                case UiKeys.LoadGame:
                    string? toLoad = await new LoadGameMenu(_uiLayer).Show().ConfigureAwait(false);
                    if(string.IsNullOrWhiteSpace(toLoad)) continue;

                    await GameHost.RunGame(toLoad, _serviceProvider).ConfigureAwait(false);
                    break;
                case UiKeys.CloseGame:
                    return;
            }
        }
    }
}