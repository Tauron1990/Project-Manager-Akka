using ServiceHost.Client.Shared.ConfigurationServer.Data;
using ServiceHost.Client.Shared.ConfigurationServer.Events;
using Tauron.Application.AkkaNode.Services.Reporting.Commands;

namespace ServiceHost.Client.Shared.ConfigurationServer;

public sealed record QueryConfigEventSource : ResultCommand<ConfigurationApi, QueryConfigEventSource, ConfigEventSource>, IConfigQuery
{
    protected override string Info => "ConfigEventSource-Query";
}

public sealed record QueryServerConfiguration : ResultCommand<ConfigurationApi, QueryServerConfiguration, ServerConfigugration>, IConfigQuery
{
    protected override string Info => "ServerConfiguration-Query";
}

public sealed record QuerySeedUrls : ResultCommand<ConfigurationApi, QuerySeedUrls, SeedUrls>, IConfigQuery
{
    protected override string Info => "SeedUrls-Query";
}

public sealed record QueryGlobalConfig : ResultCommand<ConfigurationApi, QueryGlobalConfig, GlobalConfig>, IConfigQuery
{
    protected override string Info => "GlobalConfig-Query";
}

public sealed record QuerySpecificConfig(string Id) : ResultCommand<ConfigurationApi, QuerySpecificConfig, SpecificConfig>, IConfigQuery
{
    protected override string Info => $"SpecificConfig-Query-{Id}";
}

public sealed record QueryFinalConfigData(string ApplicationName, string SoftwareName) : ResultCommand<ConfigurationApi, QueryFinalConfigData, FinalAppConfig>, IConfigQuery
{
    protected override string Info => $"FinalConfigData-Query-{ApplicationName}-{SoftwareName}";
}

public sealed record QuerySpecificConfigList : ResultCommand<ConfigurationApi, QuerySpecificConfigList, SpecificConfigList>, IConfigQuery
{
    protected override string Info => "SpecificConfigList-Query";
}