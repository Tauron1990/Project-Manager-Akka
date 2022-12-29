namespace Tauron.Application.Master.Commands.Administration.Host;

public sealed record UpdateEveryConfigurationRespond(bool Success) : OperationResponse(Success)
{
    public UpdateEveryConfigurationRespond()
        : this(false) { }
}