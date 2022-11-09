using System.Collections.Immutable;

namespace SimpleProjectManager.Shared.Services.Devices;

public sealed record Logs(ImmutableList<LogBatch> Data);