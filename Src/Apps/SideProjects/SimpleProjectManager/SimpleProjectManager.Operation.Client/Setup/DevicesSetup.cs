using SimpleProjectManager.Operation.Client.Config;
using SimpleProjectManager.Operation.Client.Core;
using SimpleProjectManager.Operation.Client.Device;

namespace SimpleProjectManager.Operation.Client.Setup;

public sealed class DevicesSetup : ISetup
{
    private readonly IClientInteraction _clientInteraction;

    public DevicesSetup(IClientInteraction clientInteraction)
        => _clientInteraction = clientInteraction;

    public async ValueTask<OperationConfiguration> RunSetup(OperationConfiguration configuration)
    {
        var newName = await _clientInteraction.Ask(configuration.Name.EmptyToNull(), "Wie Lautet der Name das Gerätes?");
        var isDevice = await _clientInteraction.Ask(configuration.Device, "Ist das der PC der Maschine?");

        if(isDevice)
        {
            var newInterface = await _clientInteraction.AskMultipleChoise(
                configuration.MachineInterface.EmptyToNull(),
                MachineInterfaces.KnowenInterfaces,
                "Welsche Maschiene wird hir betrieben?");

            return configuration with { Name = newName, Device = true, MachineInterface = newInterface };
        }
        else
            return configuration with { Name = newName, Device = false, MachineInterface = string.Empty };
    }
}