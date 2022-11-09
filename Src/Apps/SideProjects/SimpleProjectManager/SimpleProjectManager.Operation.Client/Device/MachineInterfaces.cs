using Microsoft.Extensions.DependencyInjection;
using SimpleProjectManager.Operation.Client.Device.Dummy;

namespace SimpleProjectManager.Operation.Client.Device;

public class MachineInterfaces
{
    public static readonly InterfaceId[] KnowenInterfaces =
    {
        InterfaceId.From("Dummy")
    };

    public static IMachine? Create(InterfaceId name, IServiceProvider serviceProvider)
        => name.Value switch
        {
            "Dummy" => ActivatorUtilities.CreateInstance(serviceProvider, typeof(DummyMachine)) as IMachine,
            _ => null
        };
}