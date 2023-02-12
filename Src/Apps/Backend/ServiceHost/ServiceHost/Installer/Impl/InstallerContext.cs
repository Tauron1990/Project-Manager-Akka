using System;
using System.IO;
using System.Linq;
using ServiceHost.ApplicationRegistry;
using ServiceHost.Installer.Impl.Source;
using Tauron.Application.ActorWorkflow;
using Tauron.Application.Master.Commands;
using Tauron.Application.Master.Commands.Administration.Host;

namespace ServiceHost.Installer.Impl
{
    public sealed class InstallerContext : IWorkflowContext
    {
        public InstallerContext(InstallType manual, AppName name, string softwareName, string sourceLocation, bool @override, AppType appType)
        {
            Manual = manual;
            Name = name;
            SoftwareName = softwareName;
            SourceLocation = sourceLocation;
            Override = @override;
            AppType = appType;
        }

        public Recovery Recovery { get; } = new();

        public Backup Backup { get; } = new();

        public InstallType Manual { get; }

        public AppName Name { get; }

        public object SourceLocation { get; }

        public IInstallationSource Source { get; private set; } = EmptySource.Instnace;

        public InstalledApp InstalledApp { get; private set; } = InstalledApp.Empty;

        public bool Override { get; }

        public string InstallationPath { get; set; } = string.Empty;

        public AppType AppType { get; }

        public string Exe { get; set; } = string.Empty;

        public string SoftwareName { get; }

        public IInstallationSource SetSource(Func<InstallerContext, IInstallationSource> source, Action<string> setError)
        {
            Source = source(this);
            if (Source == EmptySource.Instnace)
                setError(ErrorCodes.NoSourceFound);

            return Source;
        }

        public void SetInstalledApp(InstalledApp app)
            => InstalledApp = app;

        public string GetExe()
            => !string.IsNullOrWhiteSpace(Exe) 
                ? Exe 
                : Path.GetFileName(Directory.EnumerateFiles(InstallationPath, "*.exe", SearchOption.AllDirectories).First());
    }
}