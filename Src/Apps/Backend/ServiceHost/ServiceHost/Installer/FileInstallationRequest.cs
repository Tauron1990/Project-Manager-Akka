using ServiceHost.Installer.Impl;
using Tauron.Application.Master.Commands.Administration.Host;

namespace ServiceHost.Installer
{
    public sealed record FileInstallationRequest(string SoftwareName, string Name, string Path, bool Override, AppType AppType, string Exe) : InstallRequest;
}