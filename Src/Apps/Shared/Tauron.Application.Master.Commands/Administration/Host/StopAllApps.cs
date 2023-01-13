namespace Tauron.Application.Master.Commands.Administration.Host;

public sealed record StopAllApps(HostName Target) : InternalHostMessages.CommandBase<StopAllAppsResponse>(Target, InternalHostMessages.CommandType.AppManager);