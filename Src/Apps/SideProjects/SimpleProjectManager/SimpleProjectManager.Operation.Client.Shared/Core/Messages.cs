using SimpleProjectManager.Shared;
using Tauron.Application.AkkaNode.Services.FileTransfer;

namespace SimpleProjectManager.Operation.Client.Shared.Core;

public sealed record RegisterServerManager(DataTransferManager ServerManager);

public sealed record ServerManagerResponse(DataTransferManager ClientManager);

public sealed record SyncFile(string OperationId, ProjectFileId FileId, string FileName);

public sealed record FileChanged(ProjectFileId FileId);