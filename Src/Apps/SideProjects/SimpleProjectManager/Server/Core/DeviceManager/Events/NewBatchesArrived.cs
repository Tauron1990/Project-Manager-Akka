using SimpleProjectManager.Shared.Services.Devices;

namespace SimpleProjectManager.Server.Core.DeviceManager.Events;

public sealed record NewBatchesArrived(DeviceId Id, DateTime Date) : IDeviceEvent;