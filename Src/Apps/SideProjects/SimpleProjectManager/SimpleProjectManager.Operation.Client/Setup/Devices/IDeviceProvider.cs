using SimpleProjectManager.Operation.Client.Device;

namespace SimpleProjectManager.Operation.Client.Setup.Devices;

public interface IDeviceProvider
{
    ISetup? DeviceSetup();

    IMachine Create(IServiceProvider serviceProvider);
}