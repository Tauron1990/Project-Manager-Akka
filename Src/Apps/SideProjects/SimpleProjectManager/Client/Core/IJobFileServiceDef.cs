using RestEase;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Services;

namespace SimpleProjectManager.Client.Core;

[BasePath(ApiPaths.FilesApi)]
public interface IJobFileServiceDef
{
    [Get(nameof(GetJobFileInfo))]
    Task<ProjectFileInfo?> GetJobFileInfo([Query]ProjectFileId id, CancellationToken token);

    [Get(nameof(GetAllFiles))]
    Task<DatabaseFile[]> GetAllFiles(CancellationToken token);
    
    [Post(nameof(RegisterFile))]
    Task<string> RegisterFile([Body]ProjectFileInfo projectFile, CancellationToken token);

    [Post(nameof(UploadFiles))]
    Task<UploadFileResult> UploadFiles([Body]MultipartFormDataContent content, CancellationToken token);
    
    [Post(nameof(CommitFiles))]
    ValueTask<string> CommitFiles([Body]FileList files, CancellationToken token);

    [Post(nameof(DeleteFiles))]
    ValueTask<string> DeleteFiles([Body]FileList files, CancellationToken token);
}