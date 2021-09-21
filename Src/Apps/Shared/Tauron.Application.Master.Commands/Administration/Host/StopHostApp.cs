namespace Tauron.Application.Master.Commands.Administration.Host
{
    public sealed record StopHostApp(string AppName, string Target, InternalHostMessages.CommandType Type)
        : InternalHostMessages.CommandBase<StopHostAppResponse>(Target, Type)
    {
        public StopHostApp(string target, string appName)
            : this(appName, target, InternalHostMessages.CommandType.AppManager) { }
    }

    public sealed record StopHostAppResponse(bool Success) : OperationResponse(Success)
    {
        public StopHostAppResponse()
            : this(false) { }
    }
}