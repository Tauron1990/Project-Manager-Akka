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
        protected set => _root = value;
    }

    protected void AddWriter(Action action)
        => _writers = _writers.Add(action);

    public void RunRender()
        => _writers.ForEach(a => a());
    
    public virtual void Visit(RenderElement element)
    {
        Root = element;
        _writers = ImmutableList<Action>.Empty.Add(AnsiConsole.Clear);
        
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

    public virtual void VisitMulti(MultiElement multiElement)
    {
        foreach (RenderElement element in multiElement.Elements)
            element.Accept(this);
    }

    public void VisitCommandMenu(CommandMenu commandMenu)
    {
        var frame = new CommandFrame
                    {
                        Name = commandMenu.Name,
                    };

        NextCommandFrame(frame);

        foreach (CommandBase menuItem in commandMenu.MenuItems)
            menuItem.Accept(this);
        
        CommandFrameExit();
    }

    protected abstract void NextCommandFrame(CommandFrame frame);

    protected abstract void CommandFrameExit();
    
    public abstract void VisitCommandItem(CommandItem commandItem);
}