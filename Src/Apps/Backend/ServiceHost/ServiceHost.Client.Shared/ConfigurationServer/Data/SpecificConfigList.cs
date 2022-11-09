using System.Collections.Immutable;

namespace ServiceHost.Client.Shared.ConfigurationServer.Data;

public sealed record SpecificConfigList(ImmutableList<SpecificConfig> ConfigList);