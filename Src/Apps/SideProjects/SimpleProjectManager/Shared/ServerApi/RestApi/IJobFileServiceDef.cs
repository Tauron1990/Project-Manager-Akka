using System.Diagnostics.CodeAnalysis;
using RestEase;
using SimpleProjectManager.Shared.Services;
using Tauron.Operations;

namespace SimpleProjectManager.Shared.ServerApi.RestApi;

[BasePath(ApiPaths.FilesApi)]
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
public interface IJobFileServiceDef
{
    [Get(nameof(GetJobFileInfo))]
    Task<ProjectFileInfo?> GetJobFileInfo([Query] ProjectFileId id, CancellationToken token);

    [Get(nameof(GetAllFiles))]
    Task<DatabaseFile[]> GetAllFiles(CancellationToken token);

    [Post(nameof(RegisterFile))]
    Task<SimpleResult> RegisterFile([Body] ProjectFileInfo projectFile, CancellationToken token);

    [Post(nameof(UploadFiles))]
    Task<UploadFileResult> UploadFiles([Body] MultipartFormDataContent content, CancellationToken token);

    [Post(nameof(CommitFiles))]
    Task<SimpleResult> CommitFiles([Body] FileList files, CancellationToken token);

    [Post(nameof(DeleteFiles))]
    Task<SimpleResult> DeleteFiles([Body] FileList files, CancellationToken token);
}