namespace Tauron.Application.Master.Commands.Administration.Host;

public sealed record StopHostAppResponse(bool Success) : OperationResponse(Success)
{
    public StopHostAppResponse()
        : this(false) { }
}