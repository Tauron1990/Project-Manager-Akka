namespace ServiceHost.Client.Shared.ConfigurationServer.Data
{
    public sealed record ServerConfigugration(bool MonitorChanges, bool RestartServices, bool MergeConfigurations);
}