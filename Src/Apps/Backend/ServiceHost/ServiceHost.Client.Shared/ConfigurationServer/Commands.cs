using ServiceHost.Client.Shared.ConfigurationServer.Data;
using Tauron.Application.AkkaNode.Services.Reporting.Commands;

namespace ServiceHost.Client.Shared.ConfigurationServer
{
    public sealed record UpdateServerConfigurationCommand : SimpleCommand<ConfigurationApi, UpdateServerConfigurationCommand>, IConfigCommand
    {
        protected override string Info => "ServerConfiguration-Update";
    }

    public sealed record UpdateSeedUrlCommand(ConfigDataAction Action, SeedUrl SeedUrl, bool NoHostUpdate) : SimpleCommand<ConfigurationApi, UpdateSeedUrlCommand>, IConfigCommand
    {
        protected override string Info => $"SeedUrl-Update-{Action}-{SeedUrl}";
    }

    public sealed record UpdateGlobalConfigCommand(ConfigDataAction Action, GlobalConfig Config, bool NoHostUpdate) : SimpleCommand<ConfigurationApi, UpdateGlobalConfigCommand>, IConfigCommand
    {
        protected override string Info => $"GlobalConfig-Update-{Action}";
    }

    public sealed record UpdateSpecificConfigCommand(string Id, ConfigDataAction Action, SpecificConfig Config, bool NoHostUpdate) : SimpleCommand<ConfigurationApi, UpdateSpecificConfigCommand>, IConfigCommand
    {
        protected override string Info => $"SpecificConfig-Update-{Id}-{Action}";
    }

    public sealed record UpdateConditionCommand(string Id, ConfigDataAction Action, Condition Condition, bool NoHostUpdate) : SimpleCommand<ConfigurationApi, UpdateConditionCommand>, IConfigCommand
    {
        protected override string Info => $"Condition-Update-{Id}-{Action}-{Condition.Name}";
    }

    public sealed record ForceUpdateHostConfigCommand : SimpleCommand<ConfigurationApi, ForceUpdateHostConfigCommand>, IConfigCommand
    {
        protected override string Info => "ForceHostUpdate-Command";
    }
}