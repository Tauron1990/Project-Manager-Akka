namespace Tauron.Application.Master.Commands.Administration.Host;

public sealed record StopAllAppsResponse(bool Success) : OperationResponse(Success)
{
    public StopAllAppsResponse()
        : this(Success: false) { }
}