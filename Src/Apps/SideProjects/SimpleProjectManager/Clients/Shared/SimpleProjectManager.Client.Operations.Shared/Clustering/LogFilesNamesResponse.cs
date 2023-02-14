using System.Collections.Immutable;

namespace SimpleProjectManager.Client.Operations.Shared.Clustering;

public sealed record LogFilesNamesResponse(ImmutableList<string> Files);