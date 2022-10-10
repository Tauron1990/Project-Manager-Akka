using System.Collections.Immutable;
using Microsoft.Extensions.DependencyInjection;
using SimpleProjectManager.Operation.Client.Device.Dummy;

namespace SimpleProjectManager.Operation.Client.Device;

public class MachineInterfaces
{
    public static readonly string[] KnowenInterfaces =
    {
        "Dummy"
    };
    
    public static IMachine? Create(string name, IServiceProvider serviceProvider)
        => name switch
        {
            "Dummy" => ActivatorUtilities.CreateInstance(serviceProvider, typeof(DummyMachine)) as IMachine,
            _ => null
        };
}