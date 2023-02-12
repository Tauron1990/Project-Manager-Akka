namespace ServiceHost.ClientApp.Shared.ConfigurationServer.Data;

public sealed record ServerConfigugration(bool MonitorChanges, bool RestartServices, string Database);