using System.Collections.Immutable;

namespace SimpleProjectManager.Client.Operations.Shared.Clustering;

public sealed record LogFilesNamesResponse(string name, ImmutableList<string> Files);