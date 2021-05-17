using System;
using Tauron.Application.Master.Commands.Administration.Host;

namespace ServiceHost.ApplicationRegistry
{
    public sealed record InstalledApp(string SoftwareName, string Name, string Path, int Version, AppType AppType, string Exe)
    {
        public static readonly InstalledApp Empty = new(string.Empty, string.Empty, string.Empty, 0, AppType.Cluster, string.Empty);

        public InstalledApp NewVersion()
            => this with {Version = Version + 1};
    }
}