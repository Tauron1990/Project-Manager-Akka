using SimpleProjectManager.Client.Operations.Shared.Devices;

namespace SimpleProjectManager.Server.Core.DeviceManager;

public sealed record DeviceChanged(DeviceChangedType Type, DeviceInformations Device);