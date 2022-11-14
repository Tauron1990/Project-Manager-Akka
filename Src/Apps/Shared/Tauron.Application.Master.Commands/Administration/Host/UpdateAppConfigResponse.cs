namespace Tauron.Application.Master.Commands.Administration.Host;

public sealed record UpdateAppConfigResponse(bool Success, string App) : OperationResponse(Success)
{
    public UpdateAppConfigResponse()
        : this(Success: false, "NoAppOnError") { }
}