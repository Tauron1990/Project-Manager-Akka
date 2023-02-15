using System.Collections.Immutable;

namespace SimpleProjectManager.Client.Operations.Shared.Clustering;

public sealed record LogFilesNamesResponse(string Name, ImmutableList<string> Files);