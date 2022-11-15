using SimpleProjectManager.Shared;

namespace SimpleProjectManager.Operation.Client.ImageEditor;

public sealed record SyncFile(string OperationId, ProjectFileId FileId, string FileName);