using System.Collections.Immutable;

namespace Tauron.TextAdventure.Engine.UI.Rendering;

public sealed class CommandMenu : CommandBase
{
    public ImmutableList<CommandBase> MenuItems { get; }

    public CommandMenu(ImmutableList<CommandBase> menuItems)
        => MenuItems = menuItems;

    public CommandMenu(params CommandBase[] items)
        : this(items.ToImmutableList())
    {
        
    }

    public override void Accept(IRenderVisitor visitor)
        => visitor.VisitCommandMenu(this);
}