using SimpleProjectManager.Client.Operations.Shared.Devices;

namespace SimpleProjectManager.Server.Core.DeviceManager.Events;

public class NewDeviceEvent : IDeviceEvent
{
    public DeviceInformations Informations { get; }

    public NewDeviceEvent(DeviceInformations informations)
        => Informations = informations;

    public string DeviceName => Informations.DeviceName;
}