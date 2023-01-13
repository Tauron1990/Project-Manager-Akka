namespace Tauron.Application.Master.Commands.Administration.Host;

public sealed record UpdateSeeds(HostName Target, string[] Urls)
    : InternalHostMessages.CommandBase<UpdateSeedsResponse>(Target, InternalHostMessages.CommandType.AppRegistry);