using System.Collections.Immutable;
using ServiceHost.Client.Shared.ConfigurationServer.Data;

namespace ServiceManager.ServiceDeamon.ConfigurationServer.Data
{
    public sealed record SpecificConfigEntity(string Id, SpecificConfig Config)
    {
        public SpecificConfigEntity()
            : this(string.Empty, new SpecificConfig(string.Empty, string.Empty, string.Empty, false, ImmutableList<Condition>.Empty))
        {
            
        }
    }
}