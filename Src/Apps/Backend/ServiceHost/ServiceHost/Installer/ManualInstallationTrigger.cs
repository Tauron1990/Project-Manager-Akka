using System.IO;
using Microsoft.Extensions.Configuration;
using Tauron.Application.Master.Commands;
using Tauron.Application.Master.Commands.Administration.Host;

namespace ServiceHost.Installer
{
    public sealed class ManualInstallationTrigger
    {
        private readonly IInstaller _installer;
        private readonly ManualInstallationConfiguration _trigger = new();

        public ManualInstallationTrigger(IConfiguration config, IInstaller installer)
        {
            _installer = installer;
            config.Bind(_trigger);
        }

        public void Run()
        {
            if (_trigger.Install != InstallType.Manual) return;

            if (_trigger.AppType == AppType.Host)
                _trigger.AppName = AppName.From("Host Self Update");

            _installer.Tell(new FileInstallationRequest(_trigger.SoftwareName, _trigger.AppName, Path.GetFullPath(_trigger.ZipFile), _trigger.Override, _trigger.AppType, _trigger.Exe));
        }
    }
}