namespace Tauron.Application.Master.Commands.Administration.Host
{
    public sealed record StartAllApps : InternalHostMessages.CommandBase
    {
        public StartAllApps(string target) : base(target, InternalHostMessages.CommandType.AppManager)
        {
        }
    }
}