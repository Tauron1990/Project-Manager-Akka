using RestEase;

namespace SimpleProjectManager.Shared.ServerApi.RestApi;

[BasePath(ApiPaths.PingApi)]
public interface IPingServiceDef
{
    [Get]
    Task<string> Ping(CancellationToken token);
}