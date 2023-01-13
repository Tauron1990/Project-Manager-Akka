namespace Tauron.Application.Master.Commands.Administration.Host;

public sealed record StartHostApp(AppName AppName, HostName Target, InternalHostMessages.CommandType Type)
    : InternalHostMessages.CommandBase<StartHostAppResponse>(Target, Type)
{
    public StartHostApp(HostName target, AppName appName) : this(
        appName,
        target,
        InternalHostMessages.CommandType.AppManager)
        => AppName = appName;
}