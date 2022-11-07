using System.Collections.Immutable;

namespace SimpleProjectManager.Shared;

public sealed record UploadFileResult(SimpleMessage FailMessage, ImmutableList<ProjectFileId> Ids);