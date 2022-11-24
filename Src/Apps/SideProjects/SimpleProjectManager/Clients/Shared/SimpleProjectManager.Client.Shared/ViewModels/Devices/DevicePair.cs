using SimpleProjectManager.Shared.Services.Devices;

namespace SimpleProjectManager.Client.Shared.ViewModels.Devices;

public sealed record DevicePair(DeviceName Name, DeviceId Id);