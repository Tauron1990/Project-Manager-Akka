using System.Collections.Immutable;

namespace SimpleProjectManager.Server.Configuration.Core;

public sealed class PropertyValue
{
    public ImmutableList<NodeBase> Nodes { get; }

    public PropertyValue(ImmutableList<NodeBase> nodes)
        => Nodes = nodes;
}