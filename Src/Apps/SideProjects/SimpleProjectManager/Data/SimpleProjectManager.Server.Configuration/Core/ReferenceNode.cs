namespace SimpleProjectManager.Server.Configuration.Core;

public sealed class ReferenceNode : NodeBase
{
    public string Name { get; }

    public ReferenceNode(string name)
        => Name = name;
}