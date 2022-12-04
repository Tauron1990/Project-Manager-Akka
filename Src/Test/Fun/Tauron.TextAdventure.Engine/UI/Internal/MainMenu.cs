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
        RenderElement titleElement = _uiLayer.CreateTitle().WithTag(Tags.MainMenu);
        var menuRender = MultiElement.Create(
            titleElement);
        
        while (true)
        {
            _renderVisitor.Visit(menuRender);
            await _uiLayer.ExecutePage(_renderVisitor).ConfigureAwait(false);
            
            Console.ReadKey();
        }
    }
}