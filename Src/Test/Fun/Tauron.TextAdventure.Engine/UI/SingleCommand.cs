using JetBrains.Annotations;
using Tauron.TextAdventure.Engine.Systems;
using Tauron.TextAdventure.Engine.UI.Rendering;

namespace Tauron.TextAdventure.Engine.UI;

[PublicAPI]
public sealed class SingleCommand : CommandPairBase
{
    private readonly string _id;
    private readonly Func<IEnumerable<IGameCommand>> _getCommands;

    public SingleCommand(string id, Func<IEnumerable<IGameCommand>> getCommands)
    {
        _id = id;
        _getCommands = getCommands;
    }

    public override bool IsAsk => false;

    public override CommandBase Collect()
        => new CommandItem(_id);

    public override Func<IEnumerable<IGameCommand>>? Find(string id)
        => string.Equals(id, _id, StringComparison.Ordinal)
            ? _getCommands
            : null;
}