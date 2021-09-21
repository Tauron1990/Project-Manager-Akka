namespace Tauron.Application.Master.Commands.Administration.Host
{
    public sealed record StartAllApps : InternalHostMessages.CommandBase<StartAllAppsResponse>
    {
        public StartAllApps(string target) : base(target, InternalHostMessages.CommandType.AppManager) { }
    }

    public sealed record StartAllAppsResponse(bool Success) : OperationResponse(Success)
    {
        public StartAllAppsResponse() : this(Success: false) { }
    }
}