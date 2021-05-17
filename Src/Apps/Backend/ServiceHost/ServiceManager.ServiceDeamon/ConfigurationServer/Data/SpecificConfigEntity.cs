using System.Collections.Immutable;
using ServiceHost.Client.Shared.ConfigurationServer;
using ServiceHost.Client.Shared.ConfigurationServer.Data;

namespace ServiceManager.ServiceDeamon.ConfigurationServer.Data
{
    public sealed record SpecificConfigEntity(string Id, SpecificConfig Config)
    {
        public SpecificConfigEntity()
            : this(string.Empty, new SpecificConfig(string.Empty, string.Empty, string.Empty, ImmutableList<Condition>.Empty))
        {
            
        }

        public static SpecificConfigEntity From(UpdateSpecificConfigCommand command)
            => new(command.Id, new SpecificConfig(command.Id, command.ConfigContent, command.InfoData, ImmutableList<Condition>.Empty));
    }
}