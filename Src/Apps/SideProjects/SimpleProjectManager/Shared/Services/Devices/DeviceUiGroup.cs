using System.Collections.Immutable;

namespace SimpleProjectManager.Shared.Services.Devices;

public sealed record DeviceUiGroup(ImmutableList<DeviceUiGroup> Groups, ImmutableList<DeviceSensor> Sensors, ImmutableList<DeviceButton> DeviceButtons);