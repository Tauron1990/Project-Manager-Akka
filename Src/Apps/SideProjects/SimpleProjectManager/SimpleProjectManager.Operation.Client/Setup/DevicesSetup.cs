using SimpleProjectManager.Client.Operations.Shared;
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
        string newName = await _clientInteraction.Ask(configuration.Name.Value.EmptyToNull(), "Wie Lautet der Name das Gerätes?").ConfigureAwait(false);
        bool isDevice = await _clientInteraction.Ask(configuration.Device.Active, "Ist das der PC der Maschine?").ConfigureAwait(false);

        if(isDevice)
        {
            string newInterface = await _clientInteraction.AskMultipleChoise(
                configuration.Device.MachineInterface.Value,
                MachineInterfaces.KnowenInterfaces.Select(ii => ii.Value).ToArray(),
                "Welsche Maschiene wird hir betrieben?").ConfigureAwait(false);

            DeviceData device = configuration.Device with { Active = true, MachineInterface = InterfaceId.From(newInterface) };

            return configuration with { Device = device, Name = ObjectName.From(newName)};
        }

        DeviceData inactiveDevice = configuration.Device with { Active = false, MachineInterface = InterfaceId.Empty };
        
        return configuration with { Name = ObjectName.From(newName), Device = inactiveDevice };
    }
}