using Tauron.Application.AkkaNode.Services.Core;

namespace Tauron.Application.Master.Commands.Administration.Host
{
    public sealed record SubscribeInstallationCompled(string Target, bool Unsubscribe) 
        : InternalHostMessages.CommandBase<SubscribeInstallationCompledResponse>(Target, InternalHostMessages.CommandType.Installer);

    public sealed record SubscribeInstallationCompledResponse(EventSubscribtion Subscription, bool Success) : OperationResponse(Success)
    {
        public SubscribeInstallationCompledResponse()
            : this(EventSubscribtion.Empty, false)
        { }
    }
}