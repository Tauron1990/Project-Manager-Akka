namespace Tauron.Application.Master.Commands.Administration.Host;

public sealed record UpdateAppConfigCommand(HostName Target, AppName App, bool Restart)
    : InternalHostMessages.CommandBase<UpdateAppConfigResponse>(Target, InternalHostMessages.CommandType.AppRegistry);