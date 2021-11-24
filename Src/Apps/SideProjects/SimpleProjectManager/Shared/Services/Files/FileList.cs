using System.Collections.Immutable;

namespace SimpleProjectManager.Shared.Services;

public sealed record FileList(ImmutableList<ProjectFileId> Files);