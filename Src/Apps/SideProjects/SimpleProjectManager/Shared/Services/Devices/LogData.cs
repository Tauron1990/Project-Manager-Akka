using System.Collections.Immutable;

namespace SimpleProjectManager.Shared.Services.Devices;

public sealed record LogData(string Message, DateTime Occurance, ImmutableDictionary<string, string> Data);