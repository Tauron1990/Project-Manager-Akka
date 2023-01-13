using System.Collections.Immutable;
using JetBrains.Annotations;

namespace Tauron.TextAdventure.Engine.UI.Rendering;

[PublicAPI]
public sealed class CommandMenu : CommandBase
{
    public CommandMenu(ImmutableList<CommandBase> menuItems)
        : this(string.Empty, menuItems) { }

    public CommandMenu(params CommandBase[] items)
        : this(string.Empty, items.ToImmutableList()) { }

    public CommandMenu(string name, ImmutableList<CommandBase> menuItems)
    {
        MenuItems = menuItems;
        Name = name;
    }

    public CommandMenu(string name, params CommandBase[] items)
        : this(name, items.ToImmutableList()) { }

    public ImmutableList<CommandBase> MenuItems { get; }

    public string Name { get; }

    public override void Accept(IRenderVisitor visitor)
        => visitor.VisitCommandMenu(this);
}