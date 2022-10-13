namespace SimpleProjectManager.Server.Core.DeviceManager.Events;

public sealed record DeviceRemoved(string DeviceName) : IDeviceEvent;