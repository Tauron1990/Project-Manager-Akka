using System.Collections.Immutable;

namespace SimpleProjectManager.Shared.Services.Devices;

public sealed record DeviceList(ImmutableArray<FoundDevice> FoundDevices);