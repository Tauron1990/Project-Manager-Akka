using RestEase;

namespace SimpleProjectManager.Shared.ServerApi.RestApi;

[BasePath(ApiPaths.PingApi)]
public interface IPingServiceDef
{
    [Get]
    Task<PingResult> Ping(CancellationToken token);
}