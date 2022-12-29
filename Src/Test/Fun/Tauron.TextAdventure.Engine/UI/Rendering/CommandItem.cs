namespace Tauron.TextAdventure.Engine.UI.Rendering;

public sealed class CommandItem : CommandBase
{
    public CommandItem(string label, string id)
    {
        Label = label;
        Id = id;
    }

    public CommandItem(string id)
        : this(id, id) { }

    public string Label { get; }

    public string Id { get; }

    public override void Accept(IRenderVisitor visitor)
        => visitor.VisitCommandItem(this);
}