using SimpleProjectManager.Operation.Client.Device;
using SimpleProjectManager.Operation.Client.Device.MGI;
using Stl.DependencyInjection;

namespace SimpleProjectManager.Operation.Client.Setup.Devices;

public sealed class MgiDeviceProvider : IDeviceProvider
{
    public ISetup? DeviceSetup()
        => null;

    public IMachine Create(IServiceProvider serviceProvider)
        => serviceProvider.Activate<MgiMachine>();
}