﻿using System.Collections.Immutable;
using Cottle;
using JetBrains.Annotations;
using Spectre.Console;
using Tauron.TextAdventure.Engine.Core;
using Tauron.TextAdventure.Engine.UI;
using Tauron.TextAdventure.Engine.UI.Rendering;

namespace Tauron.TextAdventure.Engine.Console;

[PublicAPI]
public sealed class CommandFrame : IInputElement
{
    private ImmutableList<CommandItem> _commandItems = ImmutableList<CommandItem>.Empty;
    private ImmutableList<CommandFrame> _subMenus = ImmutableList<CommandFrame>.Empty;

    public string Name { get; set; } = string.Empty;

    public async ValueTask<string> Execute(AssetManager manager, Action reRender)
    {
        CommandFrame frame = this;

        while (true)
        {
            reRender();

            var selector = new SelectionPrompt<FrameItem>()
               .Title(manager.GetString(frame.Name, Context.Empty))
               .AddChoices(frame.CreateItems())
               .PageSize(10)
               .MoreChoicesText(manager.GetString(UiKeys.More, Context.Empty))
               .UseConverter(f => manager.GetString(f.Label, Context.Empty));

            FrameItem result = await selector.ShowAsync(AnsiConsole.Console, default).ConfigureAwait(false);

            if(!result.SubMenu)
                return result.Id;

            frame = frame.GetSubMenu(result.Id);
        }

    }

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