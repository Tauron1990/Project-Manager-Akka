namespace Tauron.Application.Master.Commands.Administration.Host
{
    public sealed record StopAllApps : InternalHostMessages.CommandBase<StopAllAppsResponse>
    {
        public StopAllApps(string target)
            : base(target, InternalHostMessages.CommandType.AppManager)
        {
        }
    }

    public sealed record StopAllAppsResponse(bool Success) : OperationResponse(Success)
    {
        public StopAllAppsResponse()
            : this(false)
        {
            
        }
    }
}