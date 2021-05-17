using ServiceHost.Client.Shared.ConfigurationServer.Events;

namespace ServiceManager.ServiceDeamon.ConfigurationServer.Internal
{
    public sealed record ForceHostUpdate : IConfigEvent;
}