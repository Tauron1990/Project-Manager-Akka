using SimpleProjectManager.Shared.Services.Devices;

namespace SimpleProjectManager.Server.Core.DeviceManager.Events;

public sealed record SensorUpdateEvent(DeviceId Device, DeviceId Identifer, SensorType SensorType) : IDeviceEvent;