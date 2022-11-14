namespace Tauron.Application.Master.Commands.Administration.Host;

public sealed record UpdateEveryConfiguration(HostName Target, bool Restart)
    : InternalHostMessages.CommandBase<UpdateEveryConfigurationRespond>(Target, InternalHostMessages.CommandType.AppRegistry);