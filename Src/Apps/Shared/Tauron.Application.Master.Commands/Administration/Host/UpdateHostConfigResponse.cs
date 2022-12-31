namespace Tauron.Application.Master.Commands.Administration.Host;

public sealed record UpdateHostConfigResponse(bool Success) : OperationResponse(Success)
{
    public UpdateHostConfigResponse()
        : this(Success: false) { }
}