using SimpleProjectManager.Operation.Client.Config;
using SimpleProjectManager.Operation.Client.Core;

namespace SimpleProjectManager.Operation.Client.Setup;

public sealed class IpSetup : ISetup
{
    private readonly IClientInteraction _clientInteraction;

    public IpSetup(IClientInteraction clientInteraction)
        => _clientInteraction = clientInteraction;

    public async ValueTask<OperationConfiguration> RunSetup(OperationConfiguration operationConfiguration)
    {
        string newIp = await _clientInteraction.Ask(operationConfiguration.ServerIp.Value.EmptyToNull(), "Die IP Adresse zum Projekt Server?").ConfigureAwait(false);
        int newPort = await _clientInteraction.Ask(operationConfiguration.ServerPort.Value.MinusOneToNull(), "Der Port zum Projekt Server (Stabdart: 4000)?").ConfigureAwait(false);
        int newAkkaPort = await _clientInteraction.Ask(operationConfiguration.AkkaPort.Value.MinusOneToNull(), "Der Port zum Projekt Cluster (Standart: 4001)?").ConfigureAwait(false);

        return operationConfiguration with { ServerIp = ServerIp.From(newIp), ServerPort = Port.From(newPort), AkkaPort = Port.From(newAkkaPort) };
    }
}