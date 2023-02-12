using System.Collections.Immutable;

namespace ServiceHost.ClientApp.Shared.ConfigurationServer.Data;

public sealed record SpecificConfigList(ImmutableList<SpecificConfig> ConfigList);