using JetBrains.Annotations;
using Tauron.TextAdventure.Engine.Systems;
using Tauron.TextAdventure.Engine.UI.Rendering;

namespace Tauron.TextAdventure.Engine.UI;

[PublicAPI]
public abstract class CommandPairBase
{
    public abstract bool IsAsk { get; }

    public abstract CommandBase Collect();

    public abstract Func<IEnumerable<IGameCommand>>? Find(string id);
}