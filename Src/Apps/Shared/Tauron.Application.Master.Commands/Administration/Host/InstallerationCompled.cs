namespace Tauron.Application.Master.Commands.Administration.Host;

public sealed record InstallerationCompled(bool Succesfull, string Error, AppType Type, AppName Name, InstallationAction InstallAction);