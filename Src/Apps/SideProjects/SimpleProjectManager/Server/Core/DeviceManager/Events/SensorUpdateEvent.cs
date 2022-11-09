using SimpleProjectManager.Shared.Services.Devices;

namespace SimpleProjectManager.Server.Core.DeviceManager.Events;

public sealed record SensorUpdateEvent(string DeviceName, string Identifer, SensorType SensorType) : IDeviceEvent;