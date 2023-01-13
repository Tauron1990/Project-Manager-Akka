namespace Tauron.Application.Master.Commands.Administration.Host;

public sealed record StartAllAppsResponse(bool Success) : OperationResponse(Success)
{
    public StartAllAppsResponse() : this(Success: false) { }
}