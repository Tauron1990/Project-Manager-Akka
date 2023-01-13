namespace Tauron.Application.Master.Commands.Administration.Host;

public sealed record SubscribeInstallationCompled(HostName Target, bool Unsubscribe)
    : InternalHostMessages.CommandBase<SubscribeInstallationCompledResponse>(Target, InternalHostMessages.CommandType.Installer);