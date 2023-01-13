using System.Collections.Immutable;
using JetBrains.Annotations;
using Tauron.TextAdventure.Engine.Systems;
using Tauron.TextAdventure.Engine.UI.Rendering;

namespace Tauron.TextAdventure.Engine.UI;

[PublicAPI]
public sealed class CommandPairMenu : CommandPairBase
{
    public CommandPairMenu(string name, params CommandPairBase[] subCommands)
    {
        Name = name;
        Commands = subCommands.ToImmutableList();
    }

    public ImmutableList<CommandPairBase> Commands { get; }

    public string Name { get; }

    public override bool IsAsk => false;

    public override CommandBase Collect()
        => new CommandMenu(Name, Commands.Select(c => c.Collect()).ToImmutableList());

    public override Func<IEnumerable<IGameCommand>>? Find(string id)
        =>
        (
            from command in Commands
            let function = command.Find(id)
            where function is not null
            select function
        ).FirstOrDefault();
}