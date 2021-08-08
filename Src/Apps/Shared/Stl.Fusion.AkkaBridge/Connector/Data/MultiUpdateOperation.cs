using System.Collections.Immutable;
using System.Linq;

namespace Stl.Fusion.AkkaBridge.Connector.Data
{
    public sealed record MultiUpdateOperation(ImmutableList<IRegistryOperation> Operations) : IRegistryOperation
    {
        public ServiceRegistryState Apply(ServiceRegistryState state)
            => Operations.Aggregate(state, (registryState, operation) => operation.Apply(registryState));
    }
}