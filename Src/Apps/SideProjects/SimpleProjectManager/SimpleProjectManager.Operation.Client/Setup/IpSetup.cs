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
        string newIp = await _clientInteraction.Ask(operationConfiguration.ServerIp.EmptyToNull(), "Die IP Adresse zum Projekt Server?");
        int newPort = await _clientInteraction.Ask(operationConfiguration.ServerPort.MinusOneToNull(), "Der Port zum Projekt Server (Stabdart: 4000)?");
        int newAkkaPort = await _clientInteraction.Ask(operationConfiguration.AkkaPort.MinusOneToNull(), "Der Port zum Projekt Cluster (Standart: 4001)?");

        return operationConfiguration with { ServerIp = newIp, ServerPort = newPort, AkkaPort = newAkkaPort };
    }
}