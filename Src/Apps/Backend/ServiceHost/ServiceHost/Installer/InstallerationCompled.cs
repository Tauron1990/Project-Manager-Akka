using Tauron.Application.Master.Commands.Administration.Host;

namespace ServiceHost.Installer
{
    public sealed record InstallerationCompled(bool Succesfull, string Error, AppType Type, string Name, InstallationAction InstallAction);
}