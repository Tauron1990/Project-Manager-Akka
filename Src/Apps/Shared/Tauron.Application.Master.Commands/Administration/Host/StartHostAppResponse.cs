namespace Tauron.Application.Master.Commands.Administration.Host;

public sealed record StartHostAppResponse(bool Success) : OperationResponse(Success)
{
    public StartHostAppResponse()
        : this(false) { }
}