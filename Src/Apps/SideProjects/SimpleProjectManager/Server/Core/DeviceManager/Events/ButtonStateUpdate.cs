using SimpleProjectManager.Shared.Services.Devices;

namespace SimpleProjectManager.Server.Core.DeviceManager.Events;

public sealed record ButtonStateUpdate(DeviceId Device, DeviceId Identifer) : IDeviceEvent;