using System.Diagnostics.CodeAnalysis;
using RestEase;
using SimpleProjectManager.Shared.Services.LogFiles;

namespace SimpleProjectManager.Shared.ServerApi.RestApi;

[BasePath(ApiPaths.LogsApi)]
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
public interface ILogsServiceDef
{
    [Get]
    public Task<LogFilesData> GetFileNames(CancellationToken cancel);

    [Post]
    public Task<LogFileContent> GetLogFileContent([Body] LogFileRequest request, CancellationToken cancel);
}