namespace SimpleProjectManager.Server.Configuration.Core;

public sealed class TextNode : NodeBase
{
    public string Text { get; }

    public TextNode(string text)
        => Text = text;
}