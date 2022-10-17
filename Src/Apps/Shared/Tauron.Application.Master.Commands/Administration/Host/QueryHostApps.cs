namespace Tauron.Application.Master.Commands.Administration.Host;

public sealed record QueryHostApps(HostName Target) : InternalHostMessages.CommandBase<HostAppsResponse>(Target, InternalHostMessages.CommandType.AppRegistry);