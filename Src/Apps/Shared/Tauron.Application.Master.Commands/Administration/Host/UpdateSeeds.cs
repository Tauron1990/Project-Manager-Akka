namespace Tauron.Application.Master.Commands.Administration.Host
{
    public sealed record UpdateSeedsResponse(bool Success) : OperationResponse(Success)
    {
        public UpdateSeedsResponse()
            : this(false) { }
    }

    public sealed record UpdateSeeds(string Target, string[] Urls, bool Restart) : InternalHostMessages.CommandBase<UpdateSeedsResponse>(Target, InternalHostMessages.CommandType.AppRegistry);
}