using JetBrains.Annotations;
using Tauron.TextAdventure.Engine.Systems;
using Tauron.TextAdventure.Engine.UI.Rendering;

namespace Tauron.TextAdventure.Engine.UI;

[PublicAPI]
public sealed class AskCommand : CommandPairBase
{
    private readonly string _label;
    public Func<string, IEnumerable<IGameCommand>> AskCompled { get; }

    public AskCommand(string label, Func<string, IEnumerable<IGameCommand>> askCompled)
    {
        _label = label;
        AskCompled = askCompled;

    }

    public override bool IsAsk => true;
    public override CommandBase Collect()
        => new AskElement(_label);

    public override Func<IEnumerable<IGameCommand>>? Find(string id)
        => null;
}