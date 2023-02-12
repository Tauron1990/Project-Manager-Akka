using System.Collections.Immutable;
using ServiceHost.ClientApp.Shared.ConfigurationServer;
using ServiceHost.ClientApp.Shared.ConfigurationServer.Data;
using Stl;
using Tauron;

namespace ServiceManager.ServiceDeamon.ConfigurationServer.Data
{
    public sealed record SpecificConfigEntity(string Id, SpecificConfig Config)
    {
        public SpecificConfigEntity()
            : this(string.Empty, new SpecificConfig(string.Empty, string.Empty, string.Empty, ImmutableList<Condition>.Empty)) { }

        public static Option<SpecificConfigEntity> Patch(Option<SpecificConfigEntity> entity, UpdateSpecificConfigCommand command)
            => entity.Select(
                e =>
                {
                    var newCon = command.Conditions == null
                        ? e.Config.Conditions
                        : command.Conditions.ToImmutableList();

                    return e with { Config = e.Config with { Conditions = newCon, Info = command.InfoData, ConfigContent = command.ConfigContent } };
                });

        public static SpecificConfigEntity From(UpdateSpecificConfigCommand command)
            => new(command.Id, new SpecificConfig(command.Id, command.ConfigContent, command.InfoData, command.Conditions?.ToImmutableList() ?? ImmutableList<Condition>.Empty));
    }
}