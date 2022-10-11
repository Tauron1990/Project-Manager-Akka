using System.Collections.Immutable;

namespace SimpleProjectManager.Client.Operations.Shared.Devices;

public sealed record LogData(string Message, DateTime Occurance, ImmutableDictionary<string, string> Data);