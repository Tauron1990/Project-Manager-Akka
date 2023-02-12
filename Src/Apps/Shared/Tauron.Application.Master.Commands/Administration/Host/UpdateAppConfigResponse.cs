namespace Tauron.Application.Master.Commands.Administration.Host;

public sealed record UpdateAppConfigResponse(bool Success, AppName App) : OperationResponse(Success)
{
    public UpdateAppConfigResponse()
        : this(Success: false, App: AppName.From("NoAppOnError")) { }
}