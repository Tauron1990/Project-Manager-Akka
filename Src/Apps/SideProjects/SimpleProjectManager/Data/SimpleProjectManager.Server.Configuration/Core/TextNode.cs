namespace SimpleProjectManager.Server.Configuration.Core;

public sealed class TextNode : NodeBase
{
    public TextNode(string text)
        => Text = text;

    public string Text { get; }
}