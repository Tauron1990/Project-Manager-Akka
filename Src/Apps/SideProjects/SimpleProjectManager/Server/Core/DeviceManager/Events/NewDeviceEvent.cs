using SimpleProjectManager.Client.Operations.Shared.Devices;

namespace SimpleProjectManager.Server.Core.DeviceManager.Events;

public record NewDeviceEvent(DeviceInformations Informations) : IDeviceEvent
{
    public string DeviceName => Informations.DeviceName;
}