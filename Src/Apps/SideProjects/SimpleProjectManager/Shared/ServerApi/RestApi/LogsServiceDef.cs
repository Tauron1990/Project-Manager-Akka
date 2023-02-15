using System.Diagnostics.CodeAnalysis;
using RestEase;
using SimpleProjectManager.Shared.Services.LogFiles;

namespace SimpleProjectManager.Shared.ServerApi.RestApi;

[BasePath(ApiPaths.LogsApi)]
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
public interface ILogsServiceDef
{
    [Get]
    public Task<LogFilesData> GetFileNames();

    [Post]
    public Task<LogFileContent> GetLogFileContent([Body] LogFileRequest request);
}