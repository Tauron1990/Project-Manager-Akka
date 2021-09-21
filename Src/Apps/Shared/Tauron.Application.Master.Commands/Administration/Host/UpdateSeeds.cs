namespace Tauron.Application.Master.Commands.Administration.Host
{
    public sealed record UpdateSeedsResponse(bool Success) : OperationResponse(Success)
    {
        public UpdateSeedsResponse()
            : this(Success: false) { }
    }

    public sealed record UpdateSeeds(string Target, string[] Urls) : InternalHostMessages.CommandBase<UpdateSeedsResponse>(Target, InternalHostMessages.CommandType.AppRegistry);
}