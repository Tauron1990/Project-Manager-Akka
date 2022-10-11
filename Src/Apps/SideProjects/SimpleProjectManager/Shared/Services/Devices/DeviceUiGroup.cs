using System.Collections.Immutable;

namespace SimpleProjectManager.Client.Operations.Shared.Devices;

public sealed record DeviceUiGroup(ImmutableList<DeviceUiGroup> Groups, ImmutableList<DeviceSensor> Sensors, ImmutableList<DeviceButton> DeviceButtons);