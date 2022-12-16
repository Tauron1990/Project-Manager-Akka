using JetBrains.Annotations;
using Tauron.TextAdventure.Engine.Systems;
using Tauron.TextAdventure.Engine.UI.Rendering;

namespace Tauron.TextAdventure.Engine.UI;

[PublicAPI]
public sealed class AskCommand : CommandPairBase
{
    private readonly string _label;
    private readonly Func<string, IEnumerable<IGameCommand>> _askCompled;

    public AskCommand(string label, Func<string, IEnumerable<IGameCommand>> askCompled)
    {
        _label = label;
        _askCompled = askCompled;

    }

    public override bool IsAsk => true;
    public override CommandBase Collect()
        => new AskElement(_label);

    public override Func<IEnumerable<IGameCommand>>? Find(string id)
        => null;

    public IEnumerable<IGameCommand> AskCompled(string command)
        => RunAskCompled(command);

    private IEnumerable<IGameCommand> RunAskCompled(string result)
        => _askCompled.GetInvocationList().Cast<Func<string, IEnumerable<IGameCommand>>>().SelectMany(@delegate => @delegate(result));
}