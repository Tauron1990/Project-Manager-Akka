using System.Collections.Immutable;
using SimpleProjectManager.Operation.Client.Device;
using SimpleProjectManager.Shared;

namespace SimpleProjectManager.Operation.Client.Setup.Devices;

public static class MachineInterfaces
{
    private static readonly ImmutableDictionary<InterfaceId, Func<IDeviceProvider>> KnowenInterfaces =
        ImmutableDictionary<InterfaceId, Func<IDeviceProvider>>.Empty
           .Add(InterfaceId.From("Dummy"), SimpleLazy.Create(() => new DummyDeviceProvider()));

    public static IEnumerable<InterfaceId> All()
        => KnowenInterfaces.Keys;

    public static ISetup? CreateSetup(in InterfaceId name)
        => KnowenInterfaces.TryGetValue(name, out var provider) ? provider().DeviceSetup() : null;
    
    public static IMachine? Create(in InterfaceId name, IServiceProvider serviceProvider)
        => KnowenInterfaces.TryGetValue(name, out var provider) ? provider().Create(serviceProvider) : null;
}