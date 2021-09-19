namespace Tauron.Application.Master.Commands.Administration.Host
{
    public sealed record StartHostApp(string AppName, string Target, InternalHostMessages.CommandType Type) 
        : InternalHostMessages.CommandBase<StartHostAppResponse>(Target, Type)
    {
        public StartHostApp(string target, string appName) : this(appName, target,
            InternalHostMessages.CommandType.AppManager)
            => AppName = appName;
    }

    public sealed record StartHostAppResponse(bool Success) : OperationResponse(Success)
    {
        public StartHostAppResponse()
            : this(Success: false) { }
    }
}