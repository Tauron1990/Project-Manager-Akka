using System.Collections.Immutable;

namespace SimpleProjectManager.Server.Configuration.Core;

public sealed class PropertyValue
{
    public PropertyValue(ImmutableList<NodeBase> nodes)
        => Nodes = nodes;

    public ImmutableList<NodeBase> Nodes { get; }
}