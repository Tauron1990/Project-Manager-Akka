namespace ServiceManager.Shared.Apps
{
    public sealed record DefaultAppConfig(string Name);

    public static class DefaultApps
    {
        public static readonly DefaultAppConfig[] Apps =
        {
            new("Service Host"),
            new("Master Seed Node"),
            new("Infrastruktur Service")
        };

    }
}