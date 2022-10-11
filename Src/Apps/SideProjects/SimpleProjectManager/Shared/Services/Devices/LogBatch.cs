using System.Collections.Immutable;

namespace SimpleProjectManager.Client.Operations.Shared.Devices;

public sealed record LogBatch(string DeviceName, ImmutableList<LogData> Logs);