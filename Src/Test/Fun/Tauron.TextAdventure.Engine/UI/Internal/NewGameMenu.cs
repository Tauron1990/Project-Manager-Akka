using Tauron.TextAdventure.Engine.Data;
using Tauron.TextAdventure.Engine.UI.Rendering;

namespace Tauron.TextAdventure.Engine.UI.Internal;

public sealed class NewGameMenu
{
    private readonly IUILayer _uiLayer;
    private readonly IRenderVisitor _visitor;

    public NewGameMenu(IUILayer uiLayer)
    {
        _uiLayer = uiLayer;
        _visitor = uiLayer.CreateForPage();
    }

    public ValueTask<string?> SelectName()
    {
        RenderElement ui = MultiElement
           .Create(
                _uiLayer.CreateTitle(),
                new SpacingElement { Amount = 3 },
                new TextElement("Bekannte Speicher Slots"),
                new SpacingElement(),
                MultiElement.Create(SaveHelper.GetSaveGames().Select(t => new TextElement(t))),
                new SpacingElement { Amount = 3 },
                new AskElement("Name des Speicher Standes")
            )
           .WithTag(Tags.NewGame);

        _visitor.Visit(ui);

        return _uiLayer.ExecutePage(_visitor);
    }
}