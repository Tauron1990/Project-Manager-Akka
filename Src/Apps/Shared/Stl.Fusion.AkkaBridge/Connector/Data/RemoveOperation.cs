namespace Stl.Fusion.AkkaBridge.Connector.Data
{
    public sealed record RemoveOperation(string ServiceKey, string ActorPath) : IRegistryOperation
    {
        public ServiceRegistryState Apply(ServiceRegistryState state)
            => state.Registry.TryGetValue(ServiceKey, out var set)
                ? state with { Registry = state.Registry.SetItem(ServiceKey, set.Remove(ActorPath)) }
                : state;
    }
}