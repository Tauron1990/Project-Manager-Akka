using System.Collections.Immutable;
using SimpleProjectManager.Shared;

namespace SimpleProjectManager.Client.Shared.Data.Files;

public sealed record UploadTransactionContext(ImmutableList<FileUploadFile> Files, ProjectName JobName);