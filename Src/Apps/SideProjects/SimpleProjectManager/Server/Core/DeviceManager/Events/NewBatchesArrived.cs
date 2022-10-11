namespace SimpleProjectManager.Server.Core.DeviceManager.Events;

public sealed record NewBatchesArrived(string DeviceName) : IDeviceEvent;