using System.Collections.Immutable;
using JetBrains.Annotations;
using Spectre.Console;
using Tauron.TextAdventure.Engine.Console.Render;
using Tauron.TextAdventure.Engine.UI;
using Tauron.TextAdventure.Engine.UI.Rendering;

namespace Tauron.TextAdventure.Engine.Console;

[PublicAPI]
public abstract class SpectreVisitor : IRenderVisitor
{
    private RenderElement? _root;

    private ImmutableList<Action> _writers = ImmutableList<Action>.Empty;
    
    public RenderElement Root
    {
        get
        {
            if(_root is null)
                throw new InvalidOperationException("No Root elment was Defined");
            
            return _root;
        }
        private set => _root = value;
    }
    
    
    public virtual void Visit(RenderElement element)
    {
        Root = element;
        _writers = ImmutableList<Action>.Empty;
        
        element.Accept(this);
    }

    public virtual void VisitCustom(CustomElement customElement)
    {
        switch (customElement)
        {
            case RenderableElement renderableElement:
                _writers = _writers.Add(() => AnsiConsole.Write(renderableElement.ToRender));
                break;
            default:
                throw new InvalidCastException("Unkowen Custom Element");
        }
    }
    public abstract void VisitGameTitle(GameTitleElement gameTitleElement);
    public abstract void VisitMulti(MultiElement multiElement);
}