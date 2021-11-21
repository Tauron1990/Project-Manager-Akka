using RestEase;
using SimpleProjectManager.Shared.Services;

namespace SimpleProjectManager.Client.Core;

[BasePath(ApiPaths.ErrorApi)]
public interface ICriticalErrorServiceDef
{
    [Get(nameof(CountErrors))]
    Task<long> CountErrors(CancellationToken token);
    
    [Get(nameof(GetErrors))]
    Task<CriticalError[]> GetErrors(CancellationToken token);

    [Post(nameof(DisableError))]
    Task<string?> DisableError([Query]string id, CancellationToken token);
    
    [Post(nameof(WriteError))]
    Task WriteError([Body]CriticalError error, CancellationToken token);
}