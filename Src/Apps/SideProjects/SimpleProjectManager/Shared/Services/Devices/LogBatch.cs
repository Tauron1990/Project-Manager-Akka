using System.Collections.Immutable;

namespace SimpleProjectManager.Shared.Services.Devices;

public sealed record LogBatch(DeviceId DeviceName, ImmutableList<LogData> Logs)
{
    public bool IsEmpty => Logs.IsEmpty;
}