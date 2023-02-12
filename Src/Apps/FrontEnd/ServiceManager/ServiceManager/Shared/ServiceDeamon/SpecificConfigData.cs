using System.Collections.Immutable;
using ServiceHost.ClientApp.Shared.ConfigurationServer.Data;

namespace ServiceManager.Shared.ServiceDeamon
{
    public sealed record SpecificConfigData(bool IsNew, string Name, string? Info, string Content, ImmutableList<ConfigurationInfo>? Conditions);

    public sealed record ConfigurationInfo(string Name, ConfigDataAction Action, Condition Condition);
}