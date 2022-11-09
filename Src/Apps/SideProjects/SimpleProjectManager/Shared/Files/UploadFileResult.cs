using System.Collections.Immutable;
using Tauron.Operations;

namespace SimpleProjectManager.Shared;

public sealed record UploadFileResult(SimpleResult FailMessage, ImmutableList<ProjectFileId> Ids);