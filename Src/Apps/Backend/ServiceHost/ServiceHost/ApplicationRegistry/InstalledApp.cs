using Tauron.Application.Master.Commands;
using Tauron.Application.Master.Commands.Administration.Host;

namespace ServiceHost.ApplicationRegistry
{
    public sealed record InstalledApp(string SoftwareName, AppName Name, string Path, SimpleVersion Version, AppType AppType, string Exe)
    {
        public static readonly InstalledApp Empty = new(string.Empty, AppName.Empty, string.Empty, SimpleVersion.NoVersion, AppType.Cluster, string.Empty);

        public InstalledApp NewVersion()
            => this with { Version = Version + 1 };
    }
}