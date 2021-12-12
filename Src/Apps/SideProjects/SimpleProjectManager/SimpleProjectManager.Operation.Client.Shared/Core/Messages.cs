using SimpleProjectManager.Shared;
using Tauron.Application.AkkaNode.Services.FileTransfer;

namespace SimpleProjectManager.Operation.Client.Shared.Core;

public enum ImageManagerMode
{
    Single,
    Multiple
}

public sealed record RegisterServerManager(DataTransferManager ServerManager);

public sealed record ServerManagerResponse(DataTransferManager ClientManager);

public sealed record StartProject(string JobName);

public sealed record SyncFile(string OperationId, ProjectFileId FileId, string FileName);

public sealed record FileChanged(ProjectFileId FileId);