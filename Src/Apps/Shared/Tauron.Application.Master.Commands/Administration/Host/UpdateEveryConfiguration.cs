namespace Tauron.Application.Master.Commands.Administration.Host
{
    public sealed record UpdateEveryConfiguration(string Target, bool Restart)
        : InternalHostMessages.CommandBase<UpdateEveryConfigurationRespond>(Target, InternalHostMessages.CommandType.AppRegistry) { }

    public sealed record UpdateEveryConfigurationRespond(bool Success) : OperationResponse(Success)
    {
        public UpdateEveryConfigurationRespond()
            : this(false) { }
    }
}