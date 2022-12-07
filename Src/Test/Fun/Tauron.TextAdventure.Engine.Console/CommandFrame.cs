using System.Collections.Immutable;
using JetBrains.Annotations;
using Tauron.TextAdventure.Engine.UI.Rendering;

namespace Tauron.TextAdventure.Engine.Console;

[PublicAPI]
public sealed class CommandFrame
{
    private ImmutableList<CommandFrame> _subMenus = ImmutableList<CommandFrame>.Empty;
    private ImmutableList<CommandItem> _commandItems = ImmutableList<CommandItem>.Empty;
    
    public string Name { get; set; } = string.Empty;

    public CommandFrame GetSubMenu(string name)
        => _subMenus.First(f => string.Equals(f.Name, name, StringComparison.Ordinal));
    
    public IEnumerable<FrameItem> CreateItems()
    {
        foreach (CommandFrame commandFrame in _subMenus)
            yield return new FrameItem(commandFrame.Name, commandFrame.Name, SubMenu: true);

        foreach (CommandItem item in _commandItems)
            yield return new FrameItem(item.Label, item.Id, SubMenu: false);
    }

    public void AddFrame(CommandFrame frame)
        => _subMenus = _subMenus.Add(frame);

    public void AddItem(CommandItem item)
        => _commandItems = _commandItems.Add(item);
}