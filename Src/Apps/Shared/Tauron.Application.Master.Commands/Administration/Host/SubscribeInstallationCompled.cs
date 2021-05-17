namespace Tauron.Application.Master.Commands.Administration.Host
{
    public sealed record SubscribeInstallationCompled(string Target, bool Unsubscribe) : InternalHostMessages.CommandBase(Target, InternalHostMessages.CommandType.Installer);
}