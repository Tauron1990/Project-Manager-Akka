using System.Collections.Immutable;

namespace Stl.Fusion.AkkaBridge.Connector.Data
{
    public sealed record AddOperation(string ServiceKey, string ActorPath) : IRegistryOperation
    {
        public ServiceRegistryState Apply(ServiceRegistryState state)
            => state.Registry.TryGetValue(ServiceKey, out var list) 
                ? state with { Registry = state.Registry.SetItem(ServiceKey, list.Add(ActorPath)) } 
                : state with { Registry = state.Registry.Add(ServiceKey, ImmutableHashSet<string>.Empty.Add(ActorPath)) };
    }
}