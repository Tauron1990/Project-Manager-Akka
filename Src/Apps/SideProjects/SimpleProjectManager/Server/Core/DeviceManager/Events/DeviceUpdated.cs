using SimpleProjectManager.Client.Operations.Shared.Devices;
using SimpleProjectManager.Shared.Services.Devices;

namespace SimpleProjectManager.Server.Core.DeviceManager.Events;

public sealed class DeviceUpdated : IDeviceEvent
{
    public DeviceId Device { get; }
    public DeviceInformations DeviceInformations { get; }

    public DeviceUpdated(DeviceId device, DeviceInformations deviceInformations)
    {
        Device = device;
        DeviceInformations = deviceInformations;
    }
}