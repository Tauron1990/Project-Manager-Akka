using System.Diagnostics.CodeAnalysis;
using RestEase;
using SimpleProjectManager.Shared.Services.LogFiles;

namespace SimpleProjectManager.Shared.ServerApi.RestApi;

[BasePath(ApiPaths.LogsApi)]
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
public interface ILogsServiceDef
{
    [Get(nameof(GetFileNames))]
    public Task<LogFilesData> GetFileNames(CancellationToken cancel);

    [Post(nameof(GetLogFileContent))]
    public Task<LogFileContent> GetLogFileContent([Body] LogFileRequest request, CancellationToken cancel);
}