using JetBrains.Annotations;

namespace Tauron.TextAdventure.Engine.UI.Rendering;

[PublicAPI]
public sealed class AskElement : CommandBase
{
    public string Label { get; }

    public AskElement()
        => Label = string.Empty;

    public AskElement(string label)
        => Label = label;

    public override void Accept(IRenderVisitor visitor)
        => visitor.VisitAsk(this);
}