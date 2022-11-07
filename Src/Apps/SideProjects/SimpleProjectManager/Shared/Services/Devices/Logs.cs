using System.Collections.Immutable;

namespace SimpleProjectManager.Shared.Services.Devices;

public record Logs(ImmutableList<LogBatch> Data);