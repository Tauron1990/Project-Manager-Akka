using SimpleProjectManager.Shared.Services.Devices;

namespace SimpleProjectManager.Server.Core.DeviceManager.Events;

public sealed record DeviceRemoved(DeviceId Id) : IDeviceEvent;