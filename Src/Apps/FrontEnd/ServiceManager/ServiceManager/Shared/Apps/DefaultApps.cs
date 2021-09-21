namespace ServiceManager.Shared.Apps
{
    public sealed record DefaultAppConfig(string Name, string Repository, string ProjectName);

    public static class DefaultApps
    {
        public static readonly DefaultAppConfig[] Apps =
        {
            new("Service Host", "Tauron1990/Project-Manager-Akka", "ServiceHost.csproj"),
            new("Master Seed Node", "Tauron1990/Project-Manager-Akka", "Master.Seed.Node.csproj"),
            new("Infrastruktur Service", "Tauron1990/Project-Manager-Akka", "InfrastructureService.csproj")
        };
    }
}