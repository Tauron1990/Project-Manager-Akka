using Tauron.TextAdventure.Engine.UI.Rendering;

namespace Tauron.TextAdventure.Engine.UI.Internal;

public sealed class MainMenu
{
    private readonly IUILayer _uiLayer;
    private readonly IRenderVisitor _renderVisitor;
    
    public MainMenu(IUILayer uiLayer)
    {
        _uiLayer = uiLayer;
        _renderVisitor = uiLayer.CreateForPage();
    }

    public async ValueTask Show()
    {
        RenderElement titleElement = _uiLayer.CreateTitle();
        RenderElement menuRender = MultiElement.Create(
                titleElement,
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
                    Console.WriteLine("Neues Spiel");
                    break;
                case UiKeys.LoadGame:
                    Console.WriteLine("Spiel Laden");
                    break;
                case UiKeys.CloseGame:
                    return;
            }
        }
    }
}