using Spectre.Console;
using Tauron.TextAdventure.Engine.Console;
using Tauron.TextAdventure.Engine.UI.Rendering;

namespace SpaceConqueror.UI;

public sealed class ConsoleUIVisitor : SpectreVisitor
{
    public override void VisitGameTitle(GameTitleElement gameTitleElement)
    {
        throw new NotImplementedException();
    }

    public override void VisitMulti(MultiElement multiElement)
    {
        throw new NotImplementedException();
    }
}