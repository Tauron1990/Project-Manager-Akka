using System.Collections.Immutable;

namespace SimpleProjectManager.Shared.Services.Devices;

public sealed record DeviceUiGroup(
    string Category,
    ImmutableList<DeviceUiGroup> Groups,
    ImmutableList<DeviceSensor> Sensors,
    ImmutableList<DeviceButton> DeviceButtons);