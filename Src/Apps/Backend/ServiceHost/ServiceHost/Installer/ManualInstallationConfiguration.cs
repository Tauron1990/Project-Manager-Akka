using JetBrains.Annotations;
using Tauron.Application.Master.Commands.Administration.Host;

namespace ServiceHost.Installer
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class ManualInstallationConfiguration
    {
        public InstallType Install { get; set; } = InstallType.Empty;

        public string ZipFile { get; set; } = string.Empty;

        public string AppName { get; set; } = string.Empty;

        public string SoftwareName { get; set; }

        public bool Override { get; set; }

        public string Exe { get; set; } = string.Empty;

        public AppType AppType { get; set; } = AppType.StartUp;
    }
}