﻿using JetBrains.Annotations;
using Tauron.Application.Master.Commands;
using Tauron.Application.Master.Commands.Administration.Host;

namespace ServiceHost.Installer
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class ManualInstallationConfiguration
    {
        public InstallType Install { get; set; } = InstallType.Empty;

        public string ZipFile { get; set; } = string.Empty;

        public AppName AppName { get; set; } = AppName.Empty;

        public string SoftwareName { get; set; } = string.Empty;

        public bool Override { get; set; }

        public string Exe { get; set; } = string.Empty;

        public AppType AppType { get; set; } = AppType.StartUp;
    }
}