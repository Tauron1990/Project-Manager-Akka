using System.Collections.Immutable;

namespace SimpleProjectManager.Shared;

public sealed record UploadFileResult(string FailMessage, ImmutableList<ProjectFileId> Ids);