using System.Collections.Immutable;

namespace SimpleProjectManager.Shared.Services.LogFiles;

public sealed record LogFileEntry(string HostName, ImmutableList<string> Files);