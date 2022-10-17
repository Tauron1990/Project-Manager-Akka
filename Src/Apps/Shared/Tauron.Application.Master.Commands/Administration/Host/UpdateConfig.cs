namespace Tauron.Application.Master.Commands.Administration.Host;

public sealed record UpdateHostConfigResponse(bool Success) : OperationResponse(Success)
{
    public UpdateHostConfigResponse()
        : this(Success: false) { }
}

public sealed record UpdateAppConfigResponse(bool Success, string App) : OperationResponse(Success)
{
    public UpdateAppConfigResponse()
        : this(Success: false, "NoAppOnError") { }
}

public sealed record UpdateHostConfigCommand(HostName Target) 
    : InternalHostMessages.CommandBase<UpdateHostConfigResponse>(Target, InternalHostMessages.CommandType.AppRegistry);

public sealed record UpdateAppConfigCommand(HostName Target, AppName App, bool Restart) 
    : InternalHostMessages.CommandBase<UpdateAppConfigResponse>(Target, InternalHostMessages.CommandType.AppRegistry);