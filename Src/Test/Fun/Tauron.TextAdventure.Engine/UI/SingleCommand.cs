using JetBrains.Annotations;
using Tauron.TextAdventure.Engine.Systems;
using Tauron.TextAdventure.Engine.UI.Rendering;

namespace Tauron.TextAdventure.Engine.UI;

[PublicAPI]
public sealed class SingleCommand : CommandPairBase
{
    private readonly Func<IEnumerable<IGameCommand>> _getCommands;
    private readonly string _id;
    private readonly string _label;

    public SingleCommand(string id, string label, Func<IEnumerable<IGameCommand>> getCommands)
    {
        _id = id;
        _label = label;
        _getCommands = getCommands;
    }

    public override bool IsAsk => false;

    public override CommandBase Collect()
        => new CommandItem(_label, _id);

    public override Func<IEnumerable<IGameCommand>>? Find(string id)
        => string.Equals(id, _id, StringComparison.Ordinal)
            ? _getCommands
            : null;

    private IEnumerable<IGameCommand> RunCommand()
        => _getCommands.GetInvocationList().Cast<Func<IEnumerable<IGameCommand>>>().SelectMany(f => f());
}