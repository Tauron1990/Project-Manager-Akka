using System.Collections.Immutable;

namespace SimpleProjectManager.Shared.Services.Devices;

public sealed record LogBatch(string DeviceName, ImmutableList<LogData> Logs);