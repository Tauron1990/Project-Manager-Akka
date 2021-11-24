using RestEase;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Services;

namespace SimpleProjectManager.Client.Core;

[BasePath(ApiPaths.FilesApi)]
public interface IJobFileServiceDef
{
    [Get(nameof(GetJobFileInfo))]
    Task<ProjectFileInfo?> GetJobFileInfo([Query]ProjectFileId id, CancellationToken token);

    [Post(nameof(RegisterFile))]
    Task<string> RegisterFile([Body]ProjectFileInfo projectFile, CancellationToken token);

    [Post(nameof(UploadFiles))]
    public Task<string> UploadFiles([Body]MultipartFormDataContent content, CancellationToken token);
}