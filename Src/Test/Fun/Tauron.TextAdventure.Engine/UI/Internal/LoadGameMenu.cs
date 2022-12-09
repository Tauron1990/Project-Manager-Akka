using Tauron.TextAdventure.Engine.Data;
using Tauron.TextAdventure.Engine.UI.Rendering;

namespace Tauron.TextAdventure.Engine.UI.Internal;

public sealed class LoadGameMenu
{
    private readonly IUILayer _uiLayer;
    private readonly IRenderVisitor _visitor;
    
    public LoadGameMenu(IUILayer uiLayer)
    {
        _uiLayer = uiLayer;
        _visitor = uiLayer.CreateForPage();
    }

    public ValueTask<string?> Show()
    {
        var saveFiles = SaveHelper.GetSaveGames();

        RenderElement ui = MultiElement
           .Create(
                _uiLayer.CreateTitle(),
                new SpacingElement { Amount = 3},
                new TextElement("Speicherstände"),
                new SpacingElement(),
                MultiElement.Create(saveFiles.Select(s => new CommandItem(s)))
            )
           .WithTag(Tags.LoadGame);
        
        _visitor.Visit(ui);

        return _uiLayer.ExecutePage(_visitor);
    }
}