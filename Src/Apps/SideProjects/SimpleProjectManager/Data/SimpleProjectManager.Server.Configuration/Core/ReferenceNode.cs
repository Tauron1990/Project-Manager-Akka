namespace SimpleProjectManager.Server.Configuration.Core;

public sealed class ReferenceNode : NodeBase
{
    public ReferenceNode(string name)
        => Name = name;

    public string Name { get; }
}