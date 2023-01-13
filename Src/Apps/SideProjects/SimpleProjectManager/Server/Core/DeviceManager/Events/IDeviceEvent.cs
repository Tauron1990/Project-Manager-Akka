using SimpleProjectManager.Shared.Services.Devices;

namespace SimpleProjectManager.Server.Core.DeviceManager.Events;

public interface IDeviceEvent
{
    DeviceId Device { get; }
}