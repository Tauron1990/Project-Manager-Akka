using SimpleProjectManager.Operation.Client.Device;
using SimpleProjectManager.Operation.Client.Device.Dummy;
using Stl.DependencyInjection;

namespace SimpleProjectManager.Operation.Client.Setup.Devices;

internal sealed class DummyDeviceProvider : IDeviceProvider
{
    public ISetup? DeviceSetup()
        => null;

    public IMachine Create(IServiceProvider serviceProvider)
        => serviceProvider.Activate<DummyMachine>();
}