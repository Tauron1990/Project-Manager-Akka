using System.Collections.Immutable;
using SimpleProjectManager.Shared.Services.Devices;

namespace SimpleProjectManager.Client.Shared.Data.States.Data;

public sealed record DeviceData(ImmutableDictionary<DeviceId, DeviceName> Devices)
{
    public DeviceData()
        : this(ImmutableDictionary<DeviceId, DeviceName>.Empty) { }

    public DeviceData(DeviceList list)
        : this(list.FoundDevices.ToImmutableDictionary(d => d.Id, d => d.Name)) { }
}