namespace SimpleProjectManager.Server.Core.DeviceManager.Events;

public sealed record ButtonStateUpdate(string DeviceName, string Identifer) : IDeviceEvent;