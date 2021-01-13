namespace Tauron.Application.Master.Commands.Administration.Host
{
    public sealed record StopAllApps : InternalHostMessages.CommandBase
    {
        public StopAllApps(string target) 
            : base(target, InternalHostMessages.CommandType.AppManager)
        {
        }
    }
}