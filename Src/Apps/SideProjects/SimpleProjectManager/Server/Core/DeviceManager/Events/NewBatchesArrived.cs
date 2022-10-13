namespace SimpleProjectManager.Server.Core.DeviceManager.Events;

public sealed record NewBatchesArrived(string DeviceName, DateTime Date) : IDeviceEvent;