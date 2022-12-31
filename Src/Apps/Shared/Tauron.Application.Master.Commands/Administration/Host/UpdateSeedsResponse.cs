namespace Tauron.Application.Master.Commands.Administration.Host;

public sealed record UpdateSeedsResponse(bool Success)
    : OperationResponse(Success)
{
    public UpdateSeedsResponse()
        : this(Success: false) { }
}