namespace Tauron.Application.Master.Commands.Administration.Host
{
    public sealed record QueryHostApps : InternalHostMessages.CommandBase<HostAppsResponse>
    {
        public QueryHostApps(string target)
            : base(target, InternalHostMessages.CommandType.AppRegistry)
        {
        }
    }
}