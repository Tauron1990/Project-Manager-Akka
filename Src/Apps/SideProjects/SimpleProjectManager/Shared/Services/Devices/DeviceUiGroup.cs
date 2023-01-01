using System.Collections.Immutable;

namespace SimpleProjectManager.Shared.Services.Devices;

public sealed record DeviceUiGroup(DisplayName Name, UIType Type, DeviceId Id, ImmutableList<DeviceUiGroup> Ui);