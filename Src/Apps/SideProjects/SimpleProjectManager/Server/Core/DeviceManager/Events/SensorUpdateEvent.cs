using SimpleProjectManager.Shared.Services.Devices;

namespace SimpleProjectManager.Server.Core.DeviceManager.Events;

public sealed record SensorUpdateEvent(DeviceId Id, string Identifer, SensorType SensorType) : IDeviceEvent;