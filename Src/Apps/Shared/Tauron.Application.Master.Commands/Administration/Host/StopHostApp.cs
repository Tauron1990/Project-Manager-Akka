namespace Tauron.Application.Master.Commands.Administration.Host;

public sealed record StopHostApp(AppName AppName, HostName Target, InternalHostMessages.CommandType Type)
    : InternalHostMessages.CommandBase<StopHostAppResponse>(Target, Type)
{
    public StopHostApp(HostName target, AppName appName)
        : this(appName, target, InternalHostMessages.CommandType.AppManager) { }
}