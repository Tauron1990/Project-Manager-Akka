﻿using System.Collections.Immutable;

namespace SimpleProjectManager.Shared.Services.Devices;

public readonly record struct FoundDevice(DeviceName Name, DeviceId Id);

public sealed record Devices(ImmutableArray<FoundDevice> FoundDevices);