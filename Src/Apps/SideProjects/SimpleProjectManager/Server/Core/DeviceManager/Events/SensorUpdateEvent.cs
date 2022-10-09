namespace SimpleProjectManager.Server.Core.DeviceManager.Events;

public sealed record SensorUpdateEvent(string DeviceName, string Identifer) : IDeviceEvent;