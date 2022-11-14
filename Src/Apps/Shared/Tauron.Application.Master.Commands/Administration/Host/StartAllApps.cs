namespace Tauron.Application.Master.Commands.Administration.Host;

public sealed record StartAllApps(HostName Target) : InternalHostMessages.CommandBase<StartAllAppsResponse>(Target, InternalHostMessages.CommandType.AppManager);