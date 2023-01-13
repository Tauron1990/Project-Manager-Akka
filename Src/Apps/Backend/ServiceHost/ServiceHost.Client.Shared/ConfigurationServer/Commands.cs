using ServiceHost.Client.Shared.ConfigurationServer.Data;
using Tauron.Application.AkkaNode.Services.Reporting.Commands;

namespace ServiceHost.Client.Shared.ConfigurationServer;

public sealed record UpdateServerConfigurationCommand(ServerConfigugration ServerConfigugration) : SimpleCommand<ConfigurationApi, UpdateServerConfigurationCommand>, IConfigCommand
{
    protected override string Info => "ServerConfiguration-Update";
}

public sealed record UpdateSeedUrlCommand(ConfigDataAction Action, SeedUrl SeedUrl) : SimpleCommand<ConfigurationApi, UpdateSeedUrlCommand>, IConfigCommand
{
    protected override string Info => $"SeedUrl-Update-{Action}-{SeedUrl}";
}

public sealed record UpdateGlobalConfigCommand(ConfigDataAction Action, GlobalConfig Config) : SimpleCommand<ConfigurationApi, UpdateGlobalConfigCommand>, IConfigCommand
{
    protected override string Info => $"GlobalConfig-Update-{Action}";
}

public sealed record UpdateSpecificConfigCommand(ConfigDataAction Action, string Id, string ConfigContent, string? InfoData, Condition[]? Conditions) : SimpleCommand<ConfigurationApi, UpdateSpecificConfigCommand>, IConfigCommand
{
    protected override string Info => $"SpecificConfig-Update-{Id}-{Action}";
}

public sealed record UpdateConditionCommand(string Name, ConfigDataAction Action, Condition Condition) : SimpleCommand<ConfigurationApi, UpdateConditionCommand>, IConfigCommand
{
    protected override string Info => $"Condition-Update-{Name}-{Action}-{Condition.Name}";
}

public sealed record ForceUpdateHostConfigCommand : SimpleCommand<ConfigurationApi, ForceUpdateHostConfigCommand>, IConfigCommand
{
    protected override string Info => "ForceHostUpdate-Command";
}