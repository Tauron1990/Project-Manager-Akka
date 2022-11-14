namespace Tauron.Application.Master.Commands.Administration.Host;

public sealed record UpdateHostConfigCommand(HostName Target)
    : InternalHostMessages.CommandBase<UpdateHostConfigResponse>(Target, InternalHostMessages.CommandType.AppRegistry);