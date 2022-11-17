using SimpleProjectManager.Client.Operations.Shared.Devices;
using SimpleProjectManager.Shared.Services.Devices;

namespace SimpleProjectManager.Server.Core.DeviceManager.Events;

public record NewDeviceEvent(DeviceInformations Informations) : IDeviceEvent
{
    public DeviceId Device => Informations.DeviceId;
}